using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperServer.SceneLogic
{
    /// <summary>
    /// 场景角色数据类
    /// </summary>
    public class ScenePlayer
    {
        /// <summary>
        /// 角色编号
        /// </summary>
        public string Id;
        /// <summary>
        /// 角色在场景中的X坐标
        /// </summary>
        public float X = 0;
        /// <summary>
        /// 角色在场景中的Y坐标
        /// </summary>
        public float Y = 0;
        /// <summary>
        /// 角色在场景中的Z坐标
        /// </summary>
        public float Z = 0;
        /// <summary>
        /// 角色的分数
        /// </summary>
        public int Score = 0;
    }
}
