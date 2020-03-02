using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    //TODO:Refactoring
    /// <summary>
    ///  子节点执行失败则返回成功，否则一直处理Running状态
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/LoopUntilFailure")]
    public class BTLoopUntilFailure : StatusNodeBase<NullStatus>, IBehaviorCompositeNode
    {
        [NodeOutputBT]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var res = context.BTNodeOutput();
            if (res == BehaviorStatus.Running)
            {
                context.Inactive(this);
                return BehaviorStatus.Running;
            }
            else if (res == BehaviorStatus.Failure)
            {
                return BehaviorStatus.Success;
            } else
            {
                context.Active(this);
                return BehaviorStatus.Running;
            }

        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            if(result == BehaviorStatus.Failure)
            {

                return (true, BehaviorStatus.Success);
            } else if(result == BehaviorStatus.Success)
            {
                context.Active(this);
                return (false, BehaviorStatus.None);
            }
            else
            {
                return (false, BehaviorStatus.None);
                //throw new System.Exception("Child node completion cannot be Running ");
            }
        }

        public override void OnTick(ref Context context)
        {
            var res = context.BTNodeOutput();
            if (res == BehaviorStatus.Running)
            {
                context.Inactive(this);
            } else if (res == BehaviorStatus.Failure)
            {
                context.Inactive(this);
                context.BehaviorRunningCompleted(BehaviorStatus.Success);
            }
            else
            {
                context.Active(this);
            }
        }
    }


    //[NodeInfo("BT/LoopUntilFailure")]
    //public class BTLoopUntilFailureAsset : NodeAsset<BTLoopUntilFailure>
    //{

        
    //}
}
