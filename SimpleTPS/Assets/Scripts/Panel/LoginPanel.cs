using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPanel : PanelBase
{
    private InputField idInput;
    private InputField pwInput;
    private EventListener loginBtn;
    private EventListener regBtn;

    /// <summary>
    /// 初始化面板的名称和一些层级
    /// </summary>
    /// <param name="args"></param>
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
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

        loginBtn = skinTrans.GetComponentFormChild<EventListener>("LoginButton");
        regBtn = skinTrans.GetComponentFormChild<EventListener>("RegButton");
        //1.给所有的UI 都可以添加交互

        loginBtn.onClick += OnLoginClick;
        regBtn.onClick += OnRegClick;
    }

    /// <summary>
    /// 注册按钮  
    /// </summary>
    /// <param name="e"></param>
    private void OnRegClick(PointerEventData e)
    {
        //它通过PanelMgr.instance.OpenPanel<RegPanel>("")  打开注册面板
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }

    /// <summary>
    /// 登陆按钮事件 
    /// </summary>
    /// <param name="e"></param>
    private void OnLoginClick(PointerEventData e)
    {
        // 它在连接服务端后 发送带用户名 和 密码参数的login协议
        // 当客户端受到服务端返回的Login协议 OnLoginBack被调用
        // 我们在OnLoginBack 解析传回来的参数

        //判断账号密码是否为空
        if (idInput.text == "" || pwInput.text == "")
        {
            //之后改用Tip 来提示用户
            Debug.Log("用户名密码不能为空");
            PanelMgr.instance.OpenPanel<TipPanel>("", "警告", "用户名密码不能为空");
            return;
        }

        //如果尚未连接 则发起连接
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host,port);
            PanelMgr.instance.OpenPanel<TipPanel>("", "警告", "请检查网络连接是否正常");
            Debug.Log("连接成功");
        }

        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        //禁用登陆按钮 
        //禁用输入框文本
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }

    /// <summary>
    /// 登陆协议的回调
    /// </summary>
    /// <param name="proto"></param>
    private void OnLoginBack(ProtocolBase proto)
    {
        ProtocolBytes p = (ProtocolBytes)proto;
        int start = 0;
        string protoName = p.GetString(start, ref start);
        int ret = p.GetInt(start, ref start);
        if(ret == 0)
        {
            Debug.Log("登陆成功:"+ idInput.text);
            //StartCoroutine(StartGameDelay(idInput.text));
            PanelMgr.instance.OpenPanel<RoomListPanel>("");
            GameMgr.instance.id = idInput.text;
            Close();
        }
        else
        {
            Debug.Log("登陆失败");
            PanelMgr.instance.OpenPanel<TipPanel>("", "警告", "用户名或密码错误");
        }
    }

    IEnumerator StartGameDelay(string id)
    {
        SceneManager.LoadScene("GameScene");
        yield return new WaitForSeconds(0.1f);
        Walk.instance.StartGame(id);
        Close();
    }


}