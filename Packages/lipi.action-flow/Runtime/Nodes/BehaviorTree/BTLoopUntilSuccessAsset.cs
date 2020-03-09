using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    /// <summary>
    ///  子节点执行成功则返回成功，否则一直处理Running状态
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/LoopUntilSuccess")]
    public class BTLoopUntilSuccess : StatusNodeBase<NullStatus>, IBehaviorCompositeNode
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
                case BehaviorStatus.Success:
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
                case BehaviorStatus.Success:
                    return (true, BehaviorStatus.Success);
                case BehaviorStatus.Failure:
                    context.Active(this);
                    return (false, BehaviorStatus.None);
                default:
                    return (false, BehaviorStatus.None);
                    //throw new System.Exception("Child node completion cannot be Running ");
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
                case BehaviorStatus.Success:
                    context.Inactive(this);
                    context.BehaviorRunningCompleted(BehaviorStatus.Success);
                    break;
                default:
                    context.Active(this);
                    break;
            }
        }
    }


    //[NodeInfo("BT/LoopUntilSuccess")]
    //public class BTLoopUntilSuccessAsset : NodeAsset<BTLoopUntilSuccess>
    //{


    //}
}
