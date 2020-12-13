using Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TipPanel : PanelBase
{
    private Text tipText;
    private Text valueText;

    private EventListener yesBtn;

    private string tipStr; //标题名
    private string valueStr;//内容名

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="args">标题，内容</param>
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "TipPanel";
        layer = PanelMgr.PanelLayer.Tips;
        tipStr = args[0].ToString();
        valueStr = args[1].ToString();
    }


    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        tipText = skinTrans.GetComponentFormChild<Text>("tipText");
        valueText = skinTrans.GetComponentFormChild<Text>("valueText");

        yesBtn = skinTrans.GetComponentFormChild<EventListener>("yesButton");

        tipText.text = tipStr;
        valueText.text = valueStr;
        yesBtn.onClick += OnYesButtonClick;
    }

    private void OnYesButtonClick(PointerEventData e)
    {
        Close();
    }
}
