using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    /// <summary>
    ///  子节点执行失败则返回成功，否则一直处理Running状态
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/LoopUntilFailure")]
    public class BTLoopUntilFailure : StatusNodeBase<NullStatus>, IBehaviorCompositeNode
    {
        [HideInInspector]
        public int _i; //无意义,暂时处理无数据类型序列化出错
        
        [NodeOutputBT]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var res = context.BTNodeOutput();
            switch (res)
            {
                case BehaviorStatus.Running:
                    context.Inactive(this);
                    return BehaviorStatus.Running;
                case BehaviorStatus.Failure:
                    return BehaviorStatus.Success;
                default:
                    context.Active(this);
                    return BehaviorStatus.Running;
            }
        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            switch (result)
            {
                case BehaviorStatus.Failure:
                    return (true, BehaviorStatus.Success);
                case BehaviorStatus.Success:
                    context.Active(this);
                    return (false, BehaviorStatus.None);
                default:
                    return (false, BehaviorStatus.None);
            }
        }

        public override void OnTick(ref Context context)
        {
            var res = context.BTNodeOutput();
            switch (res)
            {
                case BehaviorStatus.Running:
                    context.Inactive(this);
                    break;
                case BehaviorStatus.Failure:
                    context.Inactive(this);
                    context.BehaviorRunningCompleted(BehaviorStatus.Success);
                    break;
                default:
                    context.Active(this);
                    break;
            }
        }
    }


    //[NodeInfo("BT/LoopUntilFailure")]
    //public class BTLoopUntilFailureAsset : NodeAsset<BTLoopUntilFailure>
    //{

        
    //}
}
