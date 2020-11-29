using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegPanel : PanelBase
{

    private InputField idInput;
    private InputField pwInput;
    private EventListener closeBtn;
    private EventListener regBtn;


    /// <summary>
    /// 初始化面板的名称和一些层级
    /// </summary>
    /// <param name="args"></param>
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "RegPanel";
        layer = PanelMgr.PanelLayer.Panel;
    }

    /// <summary>
    /// 显示面板 并给组件赋值
    /// </summary>
    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        //1.Find 不能满足 我们去寻找一个未知层级的变换组件
        //2.我们只是需要 拿一个InputField  并不需要transform
        idInput = skinTrans.GetComponentFormChild<InputField>("NameInput");
        pwInput = skinTrans.GetComponentFormChild<InputField>("PWInput");

        closeBtn = skinTrans.GetComponentFormChild<EventListener>("CloseButton");
        regBtn = skinTrans.GetComponentFormChild<EventListener>("RegButton");
        //1.给所有的UI 都可以添加交互

        closeBtn.onClick += OnCloseClick;
        regBtn.onClick += OnRegClick;
    }

    private void OnRegClick(PointerEventData e)
    {
        //判断账号密码是否为空
        if (idInput.text == "" || pwInput.text == "")
        {
            //之后改用Tip 来提示用户
            Debug.Log("用户名密码不能为空");
            return;
        }

        //如果尚未连接 则发起连接
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host, port);
            Debug.Log("连接成功");
        }

        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        //禁用登陆按钮 
        //禁用输入框文本
        NetMgr.srvConn.Send(protocol, OnRegBack);
    }

    private void OnRegBack(ProtocolBase proto)
    {
        ProtocolBytes p = (ProtocolBytes)proto;
        int start = 0;
        string protoName = p.GetString(start, ref start);
        int ret = p.GetInt(start, ref start);
        if (ret == 0)
        {
            Debug.Log("注册成功");
            PanelMgr.instance.OpenPanel<LoginPanel>("");
            Close();
        }
        else
        {
            Debug.Log("注册失败");
        }
    }

    private void OnCloseClick(PointerEventData e)
    {
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        Close();
    }
}