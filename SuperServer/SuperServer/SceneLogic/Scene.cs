using SuperServer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer.SceneLogic
{
    /// <summary>
    /// 场景类 
    /// 管理一个场景中所有的玩家数据
    /// 添加（进入）和删除（离开）角色
    /// 更新场景中的玩家信息（同步 广播）
    /// </summary>
    public class Scene
    {
        //单例
        public static Scene instance;
        public Scene()
        {
            instance = this; 
        }
        /// <summary>
        /// 场景中的角色列表
        /// </summary>
        List<ScenePlayer> list = new List<ScenePlayer>();

        /// <summary>
        /// 根据ID获取ScenePlayer
        /// </summary>
        /// <param name="id">角色ID，角色名字</param>
        /// <returns></returns>
        private ScenePlayer GetScenePlayer(string id)
        {
            for(int i=0;i<list.Count;i++)
            {
                if (list[i].Id == id)
                    return list[i];
            }
            return null;
        }

        /// <summary>
        /// 添加玩家
        /// </summary>
        /// <param name="id">角色ID</param>
        public void AddPlayer(string id)
        {
            //多线程 可能会同时操作列表  需要加锁
            lock (list)
            {
                ScenePlayer p = new ScenePlayer();
                p.Id = id;
                list.Add(p);
            }
        }

        /// <summary>
        /// 删除玩家
        /// </summary>
        /// <param name="id">角色ID</param>
        public void DelPlayer(string id)
        {
            lock (list)
            {
                ScenePlayer p = GetScenePlayer(id);
                if (p != null)
                    list.Remove(p);
            }
            //广播通知 其他玩家 该角色离开
            //发送 PlayerLeave
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("PlayerLeave");
            protocol.AddString(id);
            //广播
            ServNet.instance.Broadcast(protocol);
        }

        /// <summary>
        /// 向指定玩家发送 该场景中的角色列表
        /// </summary>
        /// <param name="player">玩家</param>
        public void SendPlayerList(Player player)
        {
            int count = list.Count;
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.AddString("GetList");
            protocol.AddInt(count);
            for(int i = 0; i < count; i++)
            {
                ScenePlayer p = list[i];
                protocol.AddString(p.Id);
                protocol.AddFloat(p.X);
                protocol.AddFloat(p.Y);
                protocol.AddFloat(p.Z);
                protocol.AddInt(p.Score);
            }
            player.Send(protocol);
        }

        /// <summary>
        /// 更新角色信息
        /// </summary>
        /// <param name="id">角色ID</param>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="z">Z坐标</param>
        /// <param name="score">角色分数</param>
        public void UpdateInfo(string id,float x,float y,float z,int score)
        {
            int count = list.Count;
            ProtocolBytes protocol = new ProtocolBytes();
            ScenePlayer p = GetScenePlayer(id);
            if (p == null)
                return;
            p.X = x;
            p.Y = y;
            p.Z = z;
            p.Score = score;
        }
    }
}
