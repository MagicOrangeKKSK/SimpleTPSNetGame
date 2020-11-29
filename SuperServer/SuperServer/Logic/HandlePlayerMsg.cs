using SuperServer.Core;
using SuperServer.SceneLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer.Logic
{
    public partial class HandlePlayerMsg
    {

        public void MsgGetScore(Player player, ProtocolBase protoBase)
        {
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("GetScore");
            protocolRet.AddInt(player.data.score);
            player.Send(protocolRet);
            Console.WriteLine($"MsgGetScore " + player.id + ":" + player.data.score);
        }

        /// <summary>
        /// 添加分数
        /// </summary>
        /// <param name="player"></param>
        /// <param name="protoBase"></param>
        public void MsgAddScore(Player player, ProtocolBase protoBase)
        {
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            int score = protocol.GetInt(start, ref start);
            player.data.score += score;
            Console.WriteLine($"MsgAddScore {player.id}:{player.data.score}");
        }

        /// <summary>
        /// 获取玩家列表
        /// </summary>
        /// <param name="player"></param>
        /// <param name="protoBase"></param>
        public void MsgGetList(Player player ,ProtocolBase protoBase)
        {
            Scene.instance.SendPlayerList(player);
        }

        /// <summary>
        /// 更新信息
        /// </summary>
        /// <param name="player"></param>
        /// <param name="protoBase"></param>
        public void MsgUpdateInfo(Player player ,ProtocolBase protoBase)
        {
            //获取数值
            int start = 0;
            ProtocolBytes protocol = (ProtocolBytes)protoBase;
            string protoName = protocol.GetString(start, ref start);
            float x = protocol.GetFloat(start, ref start);
            float y = protocol.GetFloat(start, ref start);
            float z = protocol.GetFloat(start, ref start);
            int score = player.data.score;
            Scene.instance.UpdateInfo(player.id, x, y, z, score);
            //广播
            ProtocolBytes protocolRet = new ProtocolBytes();
            protocolRet.AddString("UpdateInfo");
            protocolRet.AddString(player.id);
            protocolRet.AddFloat(x);
            protocolRet.AddFloat(y);
            protocolRet.AddFloat(z);
            protocolRet.AddInt(score);
            ServNet.instance.Broadcast(protocolRet);
        }

    }
}
