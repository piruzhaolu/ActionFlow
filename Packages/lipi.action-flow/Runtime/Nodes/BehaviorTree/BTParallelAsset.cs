using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    public struct BTParallelData
    {
        public int Success;
        public int Failure;
        public int Running;
    }
    

    /// <summary>
    /// 并行控制节点,会同时调用子节点。所有子节点返回success则返回success，有一节点返回failure则直接返回failure
    /// TODO: 返回failure只是忽略running并不会中断running中的节点
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/Parallel")]
    public class BTParallel : StatusNodeBase<BTParallelData>, IBehaviorCompositeNode
    {
        [HideInInspector]
        public int _i; //无意义,暂时处理无数据类型序列化出错
        
        
        [NodeOutputBT(5)]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var status = new BTParallelData();

            for (var i = 0; i < 5; i++)
            {
                var v = context.BTNodeOutput(i);
                switch (v)
                {
                    case BehaviorStatus.Failure:
                        status.Failure += 1;
                        break;
                    case BehaviorStatus.Success:
                        status.Success += 1;
                        break;
                    case BehaviorStatus.Running:
                        status.Running += 1;
                        break;
                }
            }

            if (status.Failure > 0)
            {
                context.SetValue(this, default);
                return BehaviorStatus.Failure;
            }
            
            context.SetValue(this, status);
            return status.Running > 0 ? BehaviorStatus.Running : BehaviorStatus.Success;
        }
        
        
        

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            var status = context.GetValue(this);
            if (status.Running == 0)
            {// == 0 说明之前已经因为失败返回了
                return (false, BehaviorStatus.None);
            }
            if (result == BehaviorStatus.Failure)
            {
                context.SetValue(this, default);
                return (true, BehaviorStatus.Failure);
            } else
            {
                status.Running -= 1;
                context.SetValue(this, status);
                return status.Running <= 0 ? (true, BehaviorStatus.Success) : (false, BehaviorStatus.Running);
            }

        }

        public override void OnTick(ref Context context)
        {
            throw new System.NotImplementedException();
        }

    }


    //[NodeInfo("BT/Parallel")]
    //public class BTParallelAsset : NodeAsset<BTParallel>
    //{

    //}
}
