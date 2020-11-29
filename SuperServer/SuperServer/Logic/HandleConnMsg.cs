using SuperServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer.Logic
{
   public  class HandleConnMsg
    {
        public void MsgHeartBeat(Conn conn ,ProtocolBase protocolBase)
        {
            conn.lastTickTime = Sys.GetTimeStamp();
        }

        /// 注册
        /// str用户名 str密码
        /// -1 表示失败 0 表示成功
        public void MsgRegister(Conn conn,ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            string strFormat = "[收到注册协议]"+conn.GetAdress();
            Console.WriteLine(strFormat+" 用户名:"+id+" 密码:"+pw);
            //构建返回协议
            protocol = new ProtocolBytes();
            protocol.AddString("Register");
            if (DataMgr.instance.Register(id, pw))
            {
                protocol.AddInt(0);
            }
            else
            {
                protocol.AddInt(-1);
            }
            //创建角色
            DataMgr.instance.CreatePlayer(id);
            conn.Send(protocol);
        }

       
        //登录
        //str 用户名  str密码
        //-1 失败 0 成功
        public void MsgLogin(Conn conn,ProtocolBase protoBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protoName = protocol.GetString(start, ref start);
            string id = protocol.GetString(start, ref start);
            string pw = protocol.GetString(start, ref start);
            Console.WriteLine($"[收到登录协议]{conn.GetAdress()} 用户名:{id} 密码:{pw}");
            //构建返回协议
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("Login");
            if (!DataMgr.instance.CheckPassWord(id, pw))
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            //是否已经登录
            ProtocolBytes protocolLogout = new ProtocolBytes();
            protocolLogout.AddString("Logout");
            if (!Player.KickOff(id, protocolLogout))
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            //获取玩家数据
            PlayerData playerData = DataMgr.instance.GetPlayerData(id);
            if (playerData == null)
            {
                protocolRet.AddInt(-1);
                conn.Send(protocolRet);
                return;
            }
            conn.player = new Player(id, conn);
            conn.player.data = playerData;
            //触发事件
            ServNet.instance.handlePlayerEvent.OnLogin(conn.player);
            protocolRet.AddInt(0);
            conn.Send(protocolRet);
            return;
        }

        //返回协议：0 正常下线
        public void MsgLogout(Conn conn,ProtocolBase protoBase)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("Logout");
            protocol.AddInt(0);
            if(conn.player == null)
            {
                conn.Send(protocol);
                conn.Close();
            }
            else
            {
                conn.Send(protocol);
                conn.player.Logout();
            }
        }

    }
}
