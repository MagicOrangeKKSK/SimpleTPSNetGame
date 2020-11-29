using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 用来处理位置同步的相关内容
/// prefab是指Player的预制体
/// players是指玩家列表
/// 场景中所有角色都会记录在players列表中
/// playerID 玩家控制的角色id
/// lastMoveTime 记录上一次角色的时间 用于控制移动频率
/// AddPlayer
/// DelPlayer
/// </summary>
public class Walk : MonoBehaviour
{
    /// <summary>
    /// 角色预制体
    /// </summary>
    public GameObject prefab;
    /// <summary>
    /// 玩家列表  通过ID来快速查找指定玩家
    /// </summary>
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    string playerID;
    //上一次移动的时间
    public float lastMoveTime;
    //单例
    public static Walk instance;
    
    void Start()
    {
        instance = this;
    }

    /// <summary>
    /// 添加角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="score"></param>
    void AddPlayer(string id,Vector3 pos,int score)
    {
        GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
        Text text = player.transform.Find("Canvas/Text").GetComponent<Text>();
        text.text = id + ":" + score;
        players.Add(id, player);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id"></param>
    void DelPlayer(string id)
    {
        if (players.ContainsKey(id))
        {
            Destroy(players[id]);
            players.Remove(id);
        }
    }

    /// <summary>
    /// 更新分数
    /// </summary>
    /// <param name="id"></param>
    /// <param name="score"></param>
    public void UpdateScore(string id,int score)
    {
        GameObject player = players[id];
        if (player == null)
            return;
        Text text = player.transform.Find("Canvas/Text").GetComponent<Text>();
        text.text = id + ":" + score;
    }

    /// <summary>
    /// 更新信息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="score"></param>
    public void UpdateInfo(string id,Vector3 pos,int score)
    {
        //只更新自己的分数
        if(id == playerID)
        {
            UpdateScore(id, score);
            return;
        }

        //其他人
        if (players.ContainsKey(id))
        {
            players[id].transform.position = pos;
            UpdateScore(id, score);
        }
        //未初始化的玩家
        else
        {
            AddPlayer(id, pos, score);
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    /// <param name="id">id是玩家所控制的角色id</param>
    public void StartGame(string id)
    {
        Debug.Log("开始游戏 "+id);
        playerID = id;
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
        float x =  UnityEngine.Random.Range(-5, 5);
        float y = 0;
        float z =  UnityEngine.Random.Range(-5, 5);
        Vector3 pos = new Vector3(x, y, z);
        AddPlayer(playerID, pos, 0);
        //同步
        SendPos();
        //获取列表
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("GetList");
        NetMgr.srvConn.Send(proto,GetList);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", UpdateInfo);
        NetMgr.srvConn.msgDist.AddListener("UpdateLeave", PlayerLeave);
    }

    /// <summary>
    /// 玩家离开
    /// </summary>
    /// <param name="proto"></param>
    public void PlayerLeave(ProtocolBase proto)
    {
        ProtocolBytes p = (ProtocolBytes)proto;
        int start = 0;
        string protoName = p.GetString(start, ref start);
        string id = p.GetString(start, ref start);
        DelPlayer(id);
    }

    /// <summary>
    /// 更新信息
    /// </summary>
    /// <param name="proto"></param>
    public void UpdateInfo(ProtocolBase proto)
    {
        ProtocolBytes p = (ProtocolBytes)proto;
        int start = 0;
        string protoName = p.GetString(start, ref start);
        string id = p.GetString(start, ref start);
        float x = p.GetFloat(start, ref start);
        float y = p.GetFloat(start, ref start);
        float z = p.GetFloat(start, ref start);
        int score = p.GetInt(start, ref start);
        Vector3 pos = new Vector3(x,y,z);
        UpdateInfo(id, pos, score);
    }

    /// <summary>
    ///发送位置
    /// </summary>
    void SendPos()
    {
        GameObject player = players[playerID];
        Vector3 pos = player.transform.position;
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        NetMgr.srvConn.Send(proto);
    }

    /// <summary>
    /// 获取列表
    /// </summary>
    /// <param name="protocol"></param>
    public void GetList(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        for(int i = 0; i < count; i++)
        {
            string id = proto.GetString(start, ref start);
            float x = proto.GetFloat(start, ref start);
            float y = proto.GetFloat(start, ref start);
            float z = proto.GetFloat(start, ref start);
            int score = proto.GetInt(start, ref start);
            Vector3 pos = new Vector3(x,y,z);
            UpdateInfo(id, pos, score);
        }
    }


    /// <summary>
    /// 玩家移动
    /// </summary>
    void Move()
    {
        if (playerID == null) return;
        if (playerID == "" ) return;
        if (players[playerID] == null) return;
        if (Time.time - lastMoveTime < 0.1) return;
        lastMoveTime = Time.time;

        GameObject player = players[playerID];
        if (Input.GetKey(KeyCode.UpArrow))
        {
            player.transform.position += new Vector3(0, 0, 1);
            SendPos();
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            player.transform.position += new Vector3(0, 0, -1);
            SendPos();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            player.transform.position += new Vector3(-1, 0, 0);
            SendPos();
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            player.transform.position += new Vector3(1, 0, 0);
            SendPos();
        }
        if (Input.GetKey(KeyCode.Space))
        {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("AddScore");
            NetMgr.srvConn.Send(proto);
        }

    }

    public void Update()
    {
        Move();
    }
}
