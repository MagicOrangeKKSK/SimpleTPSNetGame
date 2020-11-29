using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Helper
{

    public static class TransformHelper
    {
        /// <summary>
        /// 从所有子级中递归查找指定名称的变换组件
        /// </summary>
        /// <param name="currentTrans">当前变换组件</param>
        /// <param name="name">名字</param>
        /// <returns></returns>
        public static Transform FindChildByName(this Transform currentTrans, string name)
        {
            var trans = currentTrans.Find(name);
            if (trans == null)
            {
                for (int i = 0; i < currentTrans.childCount; i++)
                {
                    trans = FindChildByName(currentTrans.GetChild(i), name);
                    if (trans != null)
                        return trans;
                }
            }
            return trans;
        }

        /// <summary>
        /// 从指定名称的转换组件上获取指定类型的组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="currentTrans">转换组件</param>
        /// <param name="name">转换组件的名字</param>
        /// <returns></returns>
        public static T GetComponentFormChild<T>(this Transform currentTrans, string name)
        {
            var trans = currentTrans.FindChildByName(name);
            if (trans != null)
            {
                return trans.GetComponent<T>();
            }
            return default(T);
        }


    }
}