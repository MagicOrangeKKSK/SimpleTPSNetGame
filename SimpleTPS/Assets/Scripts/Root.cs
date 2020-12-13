using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PanelMgr.instance.OpenPanel<LoginPanel>("");
        PanelMgr.instance.OpenPanel<TipPanel>("", "测试", "测试文本");
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        NetMgr.Update();
    }
}
