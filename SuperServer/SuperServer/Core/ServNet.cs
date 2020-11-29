using MySql.Data.MySqlClient;
using SuperServer.Logic;
using SuperServer.SceneLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SuperServer.Core
{
    //网络管理类 
    public class ServNet
    {
        public Socket listenfd; //监听socket
        public Conn[] conns;//客户端连接池

        //最大连接数
        public int maxConn = 50;
        public static ServNet instance; //单例
        //主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        //心跳时间
        public long heartBeatTime = 5;
        //协议
        public ProtocolBase proto;

        //消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();

        public ServNet()
        {
            instance = this;
        }
        //获取连接池索引 返回负数表示获取失败
        public int NewIndex()
        {
            if (conns == null) return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (!conns[i].IsUse)
                {
                    return i;
                }
            }
            return -1;
        }


        public void Start(string host, int port)
        {
            //初始化连接池
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++) conns[i] = new Conn();
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind 
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            listenfd.Listen(maxConn);
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");

            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;
            timer.Enabled = true; 
        }

     

        private void HandleMainTimer(object sender, ElapsedEventArgs e)
        {
            //处理心跳
            HeartBeat();
            timer.Start();
        }

        //心跳
        private void HeartBeat()
        {
           //Console.WriteLine("[主定时器执行]");
            long timeNow = Sys.GetTimeStamp();
            for(int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                if(conn.lastTickTime < timeNow - heartBeatTime)
                {
                    Console.WriteLine("[心脏骤停]"+conn.GetAdress());
                    lock (conn)
                        conn.Close();
                }
            }
        }



        //AcceptCb  连接回调
        //1 给新的连接分配Conn 
        //2 异步接收客户的数据
        //3 再次调用BeginAccept 实现循环
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0)
                {
                    socket.Close();
                    Console.WriteLine("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();
                    Console.WriteLine($"客户端连接 [{adr}] conn池ID:{index}");
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn
                        .BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                    listenfd.BeginAccept(AcceptCb, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb 失败:" + e.Message);
            }
        }

        //ReceiveCb  接收回调
        // 1 接收并处理消息  多人聊天 服务器收到消息后 转发给其他人
        // 2 如果收到了关闭信号 我们就断开连接
        // 3 调用BeginReceive 接收下一个数据
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            lock (conn)
            {
                try
                {
                    int count = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (count <= 0)
                    {
                        Console.WriteLine($"收到[{conn.GetAdress()}]断开连接");
                        conn.Close();
                        return;
                    }

                    conn.buffCount += count;
                    ProcessData(conn); //处理协议

                    //继续接收
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount, conn
                            .BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ex 收到[{conn.GetAdress()}]断开连接");
                    conn.Close();
                }
            }
        }

        private void ProcessData(Conn conn)
        {
            //小于长度字节
            if (conn.buffCount < sizeof(Int32))
                return;
            //消息长度
            Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
            conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);
            if (conn.buffCount < sizeof(Int32) + conn.msgLength)
            {
                //缓冲区数据长度 小于  协议头+协议体的长度
                return;
            }
            //处理消息
            //string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, sizeof(Int32), conn.msgLength);
            //if (str == "HeartBeat")
            //    conn.lastTickTime = Sys.GetTimeStamp();
            ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
            HandleMsg(conn, protocol);

            //Console.WriteLine("受到长度[" + conn.GetAdress() + "]:" + conn.msgLength);
            //Console.WriteLine("收到消息[" + conn.GetAdress() + "]:" + str);
            //Send(conn, str); //发送消息
            //清除已处理的消息
            int count = conn.buffCount - sizeof(Int32) - conn.msgLength;
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if (conn.buffCount > 0) //如果缓冲区还有数据  则递归处理
                ProcessData(conn);


          
        }

        private void HandleMsg(Conn conn, ProtocolBase protocol)
        {
            string name = protocol.GetName();
         // Console.WriteLine("[收到协议]" + name);
            string methodName = "Msg" + name;
            //连接协议分发
            if(conn.player == null || name =="HeartBeat"||name == "Logout")
            {
                MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
                if(mm == null)
                {
                    string str = "[警告]HandleMsg没有处理连接方法";
                    Console.WriteLine(str+methodName);
                    return;
                }
                Object[] obj = new object[] { conn, protocol };
          //      Console.WriteLine("[处理连接消息]"+conn.GetAdress()+":"+name);
                mm.Invoke(handleConnMsg, obj);
            }
            //角色协议分发
            else
            {
                MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
                if(mm == null)
                {
                    string str = "[警告]HandleMsg没有处理玩家方法";
                    Console.WriteLine(str+methodName);
                    return;
                }
                Object[] obj = new object[] { conn.player, protocol };
           //     Console.WriteLine("[处理玩家消息]" + conn.player.id + ":" + name);
                mm.Invoke(handlePlayerMsg, obj);
            }

        }





        //发送
        public void Send(Conn conn,ProtocolBase protocol)
        {
            byte[] bytes = protocol.Encode(); //获取bytes数据
            byte[] length = BitConverter.GetBytes(bytes.Length); //获取数据长度
            byte[] sendbuff = length.Concat(bytes).ToArray(); //将长度 和  数据拼接
            try
            {
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("[ServNet]Send 发送消息 "+conn.GetAdress()+":"+e.Message);
            }
        }

        //广播
        public void Broadcast(ProtocolBytes protocol)
        {
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null) continue;
                if (!conns[i].IsUse) continue;
                if (conns[i].player == null) continue;

                lock (conns[i].player)
                {
                    if (protocol != null)
                        conns[i].player.Send(protocol);
                }
            }
        }

        //关闭
        public void Close()
        {
            //遍历连接池
            for(int i = 0; i < conns.Length; i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.IsUse) continue;
                lock (conn)
                {
                    conn.Close();
                }
            }
        }


        //打印信息
        public void Print()
        {
            Console.WriteLine("===服务器登录信息===");
            for(int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                    continue;
                if (!conns[i].IsUse)
                    continue;
                string str = $"连接[{conns[i].GetAdress()}]";
                if (conns[i].player != null)
                {
                    str += $" 玩家id:{conns[i].player.id}";
                }
                Console.WriteLine(str);
            }
        }
    }
}
