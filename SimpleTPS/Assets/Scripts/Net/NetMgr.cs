using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//网络管理
public class NetMgr 
{
    public static Connection srvConn = new Connection(); //
                                                         // public static Connection talkConn = new Connection();
    public static void Update()
    {
        srvConn.Update();
        //talkConn.Update();
    }

    public static ProtocolBase GetHeartBeatProtocol()
    {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeartBeat");
        return protocol;
    }
}
