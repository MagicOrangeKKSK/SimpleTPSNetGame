using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
    public string skinPath; //皮肤路径
    public GameObject skin;//皮肤
    public PanelMgr.PanelLayer layer;  //层级
    public object[] args; //参数

    public virtual void Init(params object[] args)
    {
        this.args = args;
    }

    public virtual void OnShowing() { }
    public virtual void OnShowed() { }
    public virtual void Update() { }
    public virtual void OnClosing() { }
    public virtual void OnClosed() { }

    protected virtual void Close() {
        string name = this.GetType().ToString();
        PanelMgr.instance.ClosePanel(name);
    }

}
