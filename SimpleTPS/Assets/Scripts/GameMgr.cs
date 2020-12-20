using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    public static GameMgr instance;
    public string id = "Lee";

    private void Awake()
    {
        instance = this;
    }
}
