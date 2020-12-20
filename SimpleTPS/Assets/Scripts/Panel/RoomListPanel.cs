using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Helper;
using System;
using UnityEngine.EventSystems;

public class RoomListPanel : PanelBase
{
    [Header("玩家信息")]
    private Text idText;
    private Text scoreText;

    [Header("房间列表")]
    private GameObject roomPrefab;
    private Transform content;
    private EventListener closeBtn;
    private EventListener newBtn;
    private EventListener reflashBtn;

    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RoomListPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idText = skinTrans.GetComponentFormChild<Text>("NameText");
        scoreText = skinTrans.GetComponentFormChild<Text>("ScoreText");

        content = skinTrans.FindChildByName("Content");
        roomPrefab = content.FindChildByName("RoomPrefab").gameObject;
        roomPrefab.SetActive(false); //设为不激活 后续通过代码添加列表单元

        closeBtn = skinTrans.GetComponentFormChild<EventListener>("CloseButton");
        newBtn = skinTrans.GetComponentFormChild<EventListener>("NewButton");
        reflashBtn = skinTrans.GetComponentFormChild<EventListener>("ReflaseButton");

        //添加按钮事件
        reflashBtn.onClick += OnReflashClick;
        newBtn.onClick += OnNewRoomClick;
        closeBtn.onClick += OnCloseClick;

        //监听
        NetMgr.srvConn.msgDist.AddListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.AddListener("GetRoomList", RecvGetRoomList);

        //主动发送一次查询
        //Send *** Proto
        SendRoomListProto();
        SendGetAchieveProto();


        ////测试数据 
        //ProtocolBytes p = new ProtocolBytes("GetRoomList");
        //p.AddInt(2);//2个房间
        //p.AddInt(3);//3号房
        //p.AddInt(3);//3个玩家
        //p.AddInt(1); //准备中

        //p.AddInt(5);//3号房
        //p.AddInt(4);//3个玩家
        //p.AddInt(2); //战斗中
        //RecvGetRoomList(p);
    } 





    /// <summary>
    /// 刷新房间列表
    /// </summary>
    /// <param name="protocol"></param>
    private void RecvGetRoomList(ProtocolBase protocol)
    {
        //清理
        ClearRoomList();
        //解析
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        for(int i = 0; i < count; i++)
        {
            int id = proto.GetInt(start, ref start);
            int number = proto.GetInt(start, ref start);
            int status = proto.GetInt(start, ref start);
            GenerateRoomUint(id, number, status);
        }
    }

    /// <summary>
    /// 创建房间单元
    /// </summary>
    /// <param name="id">房间编号</param>
    /// <param name="number">房间中玩家的数量</param>
    /// <param name="status">房间的状态 1-准备中 2-战斗中</param>
    private void GenerateRoomUint(int id, int number, int status)
    {
        //添加房间
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(0, (id + 1) * 110);
        GameObject obj = Instantiate(roomPrefab);
        obj.transform.SetParent(content);
        obj.SetActive(true);
        //房间信息
        Transform tran = obj.transform;
        Text nameText = tran.GetComponentFormChild<Text>("RoomIDText");
        Text countText = tran.GetComponentFormChild<Text>("PlayerNumText");
        Text statusText = tran.GetComponentFormChild<Text>("RoomStatusText");

        nameText.text = $"房间号:{id}";
        countText.text = $"人数:{number}";
        statusText.text = $"状态:{(status==1?"准备中":"开战中")}";
        statusText.color = (status == 1 ? Color.black : Color.red);
        //按钮事件
        EventListener btn = tran.GetComponentFormChild<EventListener>("JoinButton");
        btn.name = id.ToString();
        btn.onClick += e =>
        {
            OnJoinButtonClick(id);
        };
    }

    /// <summary>
    /// 点击刷新按钮
    /// </summary>
    /// <param name="e"></param>
    public void OnReflashClick(PointerEventData e)
    {
        NetMgr.srvConn.Send("GetRoomList");
    }

    /// <summary>
    /// 新建房间
    /// </summary>
    /// <param name="e"></param>
    private void OnNewRoomClick(PointerEventData e)
    {
        //ProtocolBytes proto = new ProtocolBytes();
        //proto.AddString("CreateRoom");
        //NetMgr.srvConn.Send(proto, OnNewRoomBack);
        NetMgr.srvConn.Send("CreateRoom", OnNewRoomBack);
    }

    /// <summary>
    /// 新建按钮返回
    /// </summary>
    /// <param name="protocol"></param>
    private void OnNewRoomBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if(ret == 0)
        {
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "提示", "新建房间失败");
        }
    }

    /// <summary>
    /// 点击加入房间按钮
    /// </summary>
    /// <param name="id"></param>
    private void OnJoinButtonClick( int id)
    {
        ProtocolBytes protocol = new ProtocolBytes("EnterRoom");
        protocol.AddInt(id);
        NetMgr.srvConn.Send(protocol, OnJoinBtnBack);
    }

    /// <summary>
    /// 加入按钮返回
    /// </summary>
    /// <param name="protocol"></param>
    private void OnJoinBtnBack(ProtocolBase protocol)
    {
        //解析参数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0)
        {
            PanelMgr.instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else
        {
            PanelMgr.instance.OpenPanel<TipPanel>("", "提示", "进入房间失败");
        }
    }

    /// <summary>
    /// 登出协议
    /// </summary>
    /// <param name="e"></param>
    private void OnCloseClick(PointerEventData e)
    {
        NetMgr.srvConn.Send("Logout",OnCloseBack);
    }

    /// <summary>
    /// 登出返回
    /// </summary>
    /// <param name="protocol"></param>
    private void OnCloseBack(ProtocolBase protocol)
    {
        PanelMgr.instance.OpenPanel<TipPanel>("", "提示", "退出成功!");
        PanelMgr.instance.OpenPanel<LoginPanel>("","");
        Close();
        NetMgr.srvConn.Close();
    }

    /// <summary>
    /// 清理房间列表
    /// </summary>
    public void ClearRoomList()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            //RoomPrefab 是挂在Content上 所以删除之前判断一下
            if (content.GetChild(i).name.Contains("Clone"))
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }


    /// <summary>
    /// 收到GetAchieve协议
    /// </summary>
    /// <param name="proto"></param>
    private void RecvGetAchieve(ProtocolBase proto)
    {
        ProtocolBytes p = (ProtocolBytes)proto;
        int start = 0;
        string protoName = p.GetString(start,ref start);
        string id = p.GetString(start, ref start);
        int score = p.GetInt(start, ref start);

        idText.text = id;
        scoreText.text = score.ToString();
    }

    public override void OnClosing()
    {
        NetMgr.srvConn.msgDist.DelListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.DelListener("GetRoomList", RecvGetRoomList);
    }

    /// <summary>
    /// 获取个人信息
    /// </summary>
    private void SendGetAchieveProto()
    {
        NetMgr.srvConn.Send("GetAchieve");
    }

    /// <summary>
    /// 获取房间列表
    /// </summary>
    private  void SendRoomListProto()
    {
        NetMgr.srvConn.Send("GetRoomList");
    }
}
