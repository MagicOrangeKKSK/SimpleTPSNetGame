using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMgr : MonoBehaviour
{
    //单例
    public static PanelMgr instance;

    private GameObject canvas; //面板
    public Dictionary<string, PanelBase> dict;  //已经打开的面板
    private Dictionary<PanelLayer, Transform> layerDict;//各个层级所对应的父物体

    public void Awake()
    {
        instance = this;
        InitLayer();
        dict = new Dictionary<string, PanelBase>();
        DontDestroyOnLoad(gameObject);
    }

    private void InitLayer()
    {
        canvas = GameObject.Find("Canvas");
        if(canvas == null)
        {
            Debug.LogError("[PanelMgr]InitLayer 未找到Canvas");
        }

        layerDict = new Dictionary<PanelLayer, Transform>();
        foreach(PanelLayer pl in Enum.GetValues(typeof(PanelLayer)))
        {
            string name = pl.ToString();
            Transform transform = canvas.transform.Find(name);
            layerDict.Add(pl, transform);
        }
    }

    //打开面板
    public void OpenPanel<T>(string skinPath,params object[] args) where T : PanelBase
    {
        //已经打开
        string name = typeof(T).ToString();
        if (dict.ContainsKey(name))
            return;
        //面板脚本
        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dict.Add(name, panel);
        //加载皮肤
        skinPath = (skinPath != "" ? skinPath : panel.skinPath);
        GameObject skin = Resources.Load<GameObject>(skinPath);
        if (skin == null)
        {
            Debug.LogError($"[PanelMgr]OpenPanel<{typeof(T).ToString()}> Skin is Null ,SkinPath = {skinPath}");
        }
        panel.skin = Instantiate(skin);
        //坐标
        Transform skinTrans = panel.skin.transform;
        PanelLayer layer = panel.layer;
        Transform parent = layerDict[layer];
        skinTrans.SetParent(parent, false);
        //panel的生命周期
        panel.OnShowing();
        //动画 之后实现
        panel.OnShowed();
    }

    // 关闭面板
    public void ClosePanel(string name)
    {
        PanelBase panel = (PanelBase)dict[name];
        if (panel == null)
            return;

        panel.OnClosing();
        dict.Remove(name);
        panel.OnClosed();   //从字典中去除
        GameObject.Destroy(panel.skin); //销毁皮肤
        Component.Destroy(panel);  //销毁组件
    }

    public static void ShowTip(string text)
    {
        instance.OpenPanel<TipPanel>("", "提示", text);
    }

    public enum PanelLayer
    {
        //面板
        Panel,
        //提示
        Tips
    }
}
