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
    /// 并行控制节点。所有子节点返回success则返回success，有一节点返回failure则直接返回failure
    /// TODO: 返回failure只是忽略running并不会中断running中的节点
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/Parallel")]
    public class BTParallel : StatusNodeBase<BTParallelData>, IBehaviorCompositeNode
    {
        [NodeOutputBT(10)]
        public NullStatus[] Childs;


        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var status = new BTParallelData();

            for (int i = 0; i < Childs.Length; i++)
            {
                var v = context.BTNodeOutput(i);
                if (v == BehaviorStatus.Failure)
                {
                    status = new BTParallelData();
                    context.SetValue(this, status);
                    return BehaviorStatus.Failure;
                } else if (v == BehaviorStatus.Success)
                {
                    status.Success += 1;
                }else if(v == BehaviorStatus.Running)
                {
                    status.Running += 1;
                }
            }
            context.SetValue(this, status);
            if (status.Running > 0)
            {
                return BehaviorStatus.Running;
            }
            else
            {
                return BehaviorStatus.Success;
            }
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
                context.SetValue(this, new BTParallelData());
                return (true, BehaviorStatus.Failure);
            } else
            {
                status.Running -= 1;
                context.SetValue(this, status);
                if (status.Running <= 0)
                {
                    return (true, BehaviorStatus.Success);
                }
                else
                {
                    return (false, BehaviorStatus.Running);
                }
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
