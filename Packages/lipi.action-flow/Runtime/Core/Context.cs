using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace ActionFlow
{
    public struct Context 
    {
        /// <summary>
        /// 当前执行Action的Entity
        /// </summary>
        public Entity CurrentEntity { set; get; }

        /// <summary>
        /// 执行作用到的目标
        /// </summary>
        public Entity TargetEntity { set; get; }


        public EntityManager EM { set; get; }


        public int Index { set; get; }

        /// <summary>
        /// 当前节点图
        /// </summary>
        public GraphAsset Graph { set; get; }


        //====================================



    }

}
