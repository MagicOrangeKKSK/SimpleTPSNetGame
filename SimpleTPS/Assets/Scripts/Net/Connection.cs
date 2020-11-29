using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public class Connection  
{
    //常量
    const int BUFFER_SIZE = 1024;
    //Socket
    private Socket socket;
    //Buff
    private byte[] readBuff = new byte[BUFFER_SIZE];
    private int buffCount;
    //粘包分包
    private int msgLength;
    private byte[] lenBytes = new byte[sizeof(Int32)];
    //协议
    public ProtocolBase proto;
    //心跳的间隔
    public float lastTickTime = 0; //上次心跳的时间
    public float heartBeatTime = 2;
    //消息分发
    public MsgDistribution msgDist = new MsgDistribution();
    //状态 
    public enum Status
    {
        Node,
        Connected,
    };
    public Status status = Status.Node;

    //连接服务器  socket->connect->receive
    public bool Connect(string host,int port)
    {
        try
        {
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            socket.Connect(host, port);
            socket.BeginReceive(readBuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None,ReceiveCb,readBuff);
            Debug.Log("连接成功");
            //状态
            status = Status.Connected;
            return false;
        }
        catch (Exception e)
        {
            Debug.Log("连接失败:" + e.Message);
            return false;
        }
    }

    //关闭服务器
    public bool Close()
    {
        try
        {
            socket.Close();
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("关闭失败:" + e.Message);
            return false;
        }
    }

    //异步回调
    private void ReceiveCb(IAsyncResult ar)
    {
        try
        {
            //处理缓冲区
            int count = socket.EndReceive(ar); 
            buffCount = buffCount + count; 
            //处理消息
            ProcessData();
            //再次调用BeginReceive 接收消息
            socket.BeginReceive(readBuff, buffCount, BUFFER_SIZE - buffCount, SocketFlags.None, ReceiveCb, readBuff);
        }
        catch (Exception e)
        {
            Debug.Log("ReceiveCB失败:" + e.Message);
            status = Status.Node;
        }
    }

    //消息处理
    private void ProcessData()
    {
        //粘包分包处理 (协议头是四位长度)
        if (buffCount < sizeof(Int32))
            return;
        //包体长度
        Array.Copy(readBuff, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (buffCount < msgLength + sizeof(Int32))
            return;

        //协议解码
        ProtocolBase protocol = proto.Decode(readBuff, sizeof(Int32), msgLength);
        Debug.Log("收到消息 " + protocol.GetDesc());
        lock (msgDist.msgList)
        {
            msgDist.msgList.Add(protocol);
        }
        //清除已经处理的消息 
        int count = buffCount - msgLength - sizeof(Int32);
        Array.Copy(readBuff, sizeof(Int32) + msgLength, readBuff, 0, count);
        buffCount = count;
        if (buffCount > 0)
        {
            ProcessData();
        }
    }

    //发送协议
    public bool Send(ProtocolBase protocol)
    {
        if(status != Status.Connected)
        {
            Debug.LogError("[Connection]还没连接就发送数据");
            return true;
        }
        byte[] b = protocol.Encode();
        byte[] length = BitConverter.GetBytes(b.Length);
        byte[] sendBuff = length.Concat(b).ToArray();
        socket.Send(sendBuff);
        Debug.Log("发送消息 " + protocol.GetDesc());
        return true;
    }

    //发送协议
    public bool Send(ProtocolBase protocol,string cbName,MsgDistribution.Delegate cb)
    {
        if (status != Status.Connected) return false;
        msgDist.AddOnceListener(cbName, cb);
        return Send(protocol);
    }

    public bool Send(ProtocolBase protocol,MsgDistribution.Delegate cb)
    {
        string cbName = protocol.GetName();
        return Send(protocol, cbName, cb);
    }

    public void Update()
    {
        //消息
        msgDist.Update();
        //心跳
        if(status == Status.Connected)
        {
            if(Time.time - lastTickTime > heartBeatTime)
            {
                Debug.Log("发送心跳包");
                ProtocolBase protocol = NetMgr.GetHeartBeatProtocol();
                Send(protocol);
                lastTickTime = Time.time;
            }
        }
    }
}
