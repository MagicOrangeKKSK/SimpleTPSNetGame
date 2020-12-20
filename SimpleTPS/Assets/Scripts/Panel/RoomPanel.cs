using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Helper;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 房间面板
/// </summary>
public class RoomPanel : PanelBase
{
    private List<Transform> prefabs = new List<Transform>();
    private EventListener exitBtn;
    private EventListener startBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;

        for(int i =0; i< 6; i++)
        {
            string name = $"PlayerPrefab{i}";
            Transform prefab = skinTrans.FindChildByName(name);
            prefabs.Add(prefab);
        }

        exitBtn = skinTrans.GetComponentFormChild<EventListener>("ExitButton");
        startBtn = skinTrans.GetComponentFormChild<EventListener>("StartButton");

        exitBtn.onClick += OnExitClick;
        startBtn.onClick += OnStartClick;

        //监听
        NetMgr.srvConn.msgDist.AddListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.AddListener("Fight", RecvFight);

        //SendGetRoomInfoProto();

        //测试数据 
        ProtocolBytes p = new ProtocolBytes("GetRoomInfo");
        p.AddInt(2);//2个房间

        p.AddString("A");
        p.AddInt(1);
        p.AddInt(100);
        p.AddInt(1);

        p.AddString("B");
        p.AddInt(2);
        p.AddInt(1000);
        p.AddInt(0);


        RecvGetRoomInfo(p);
    }

    private void RecvFight(ProtocolBase protocol)
    {
        //MultiBattle.instance.StartBattle(proto);
        Close();
    }

    private void OnStartClick(PointerEventData e)
    {
        NetMgr.srvConn.Send("StartFight", OnStartBack);
    }

    private void OnStartBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret != 0)
        {
            PanelMgr.ShowTip("开始失败！");
        }
    }

    private void OnExitClick(PointerEventData e)
    {
        NetMgr.srvConn.Send("LeaveRoom", OnExitBack);
    }

    private void OnExitBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0)
        {
            PanelMgr.ShowTip("退出成功");
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            Close();
        }
        else
        {
            PanelMgr.ShowTip("退出失败");
        }
    }

    /// <summary>
    /// 刷新房间信息
    /// </summary>
    /// <param name="protocol"></param>
    private void RecvGetRoomInfo(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        int i = 0;
        for (i = 0; i < count; i++)
        {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            int score = proto.GetInt(start, ref start);
            int isOwner = proto.GetInt(start, ref start);
            //信息处理
            Transform tran = prefabs[i];
            Text text = tran.GetComponentFormChild<Text>("Text");
            string str = $"名字:{id}\r\n";
            str += $"阵营:{(team == 1 ? "红" : "蓝")}\r\n";
            str += $"分数:{score}\r\n";
            //是否本机
            if(id == GameMgr.instance.id)
                str += "【自己】";
            if (isOwner == 1)
                str += "【房主】";
            text.text = str;

            //不同颜色 不同队伍
            Image img = tran.GetComponent<Image>();
            img.color = team == 1 ? Color.red : Color.blue;
        }
        for (; i < 6; i++)
        {
            Transform tran = prefabs[i];
            Text text = tran.GetComponentFormChild<Text>("Text");
            text.text = "【等待玩家】";
            Image img = tran.GetComponent<Image>();
            img.color = Color.gray;
        }
    }

    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.DelListener("Fight", RecvFight);
    }

    public void SendGetRoomInfoProto()
    {
        NetMgr.srvConn.Send("GetRoomInfo");
    }



}
