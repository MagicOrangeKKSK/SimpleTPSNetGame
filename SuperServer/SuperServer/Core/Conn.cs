using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer.Core
{
     public class Conn
    {
        //表示这个连接的 缓冲区大小
        public const int BUFFER_SIZE = 1024;
        public Socket socket;
        //是否使用
        public bool IsUse = false;//是否使用
        public byte[] readBuff = new byte[BUFFER_SIZE];
        public int buffCount = 0;
        //粘包 分包
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        public int msgLength = 0;
        //心跳时间
        public long lastTickTime = long.MinValue;
        //对应的Player
        public Player player;


        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
        }

        //初始化 在启动连接的时候 会调用该方法，从而给一些变量赋值
        public void Init(Socket socket)
        {
            this.socket = socket;
            IsUse = true;
            buffCount = 0;
            lastTickTime = Sys.GetTimeStamp();
        }

        //缓冲区剩余的字节数
        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        //获取客户端的地址  
        public string GetAdress()
        {
            if (!IsUse) return "无法获取地址";
            return socket.RemoteEndPoint.ToString();
        }

        //关闭
        public void Close()
        {
            if (!IsUse) return;
            Console.WriteLine("[断开连接]" + GetAdress());
            socket.Close();
            IsUse = false;
            player = null;
        }

        //发送
        public void Send(ProtocolBase proto)
        {
            ServNet.instance.Send(this, proto);
        }
    }
}
