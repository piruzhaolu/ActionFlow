using UnityEngine;
using System.Collections;
using ActionFlow;

namespace ActionFlow
{
    /// <summary>
    /// 始终返回Failure的节点
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/AlwaysFailure")]
    public class BTAlwaysFailure : INode,  IBehaviorCompositeNode
    {
        [NodeOutputBT]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var status = context.BTNodeOutput();
            if (status == BehaviorStatus.Running)
            {
                return BehaviorStatus.Running;
            } else
            {
                return BehaviorStatus.Failure;
            }
        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            if (result == BehaviorStatus.Running)
            {
                return (true, result);
            }
            else
            {
                return (true, BehaviorStatus.Failure);
            }
        }
    }

}
