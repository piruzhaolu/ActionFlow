using UnityEngine;
using System;

namespace ActionFlow
{
    public struct BTSelectorData
    {
        public int RunningIndex;
    }

    /// <summary>
    /// 选择节点。 执行到返回Success节点时结束并返回Success，全部节点执行完毕如果没返回Success则Failure
    /// </summary>
    [Serializable]
    [NodeInfo("BT/Selector")]
    public class BTSelector : StatusNodeBase<BTSelectorData>, IBehaviorCompositeNode
    {
        // [HideInActionInspector]
        // public NullStatus[] Childs;

        [NodeOutputBT(5)]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            return ExecuteNext(ref context, -1);
        }
        
        private BehaviorStatus ExecuteNext(ref Context context, int i)
        {
            while (true)
            {
                var next = context.BTNextNode(i);
                if (!next.Valid)
                {
                    return BehaviorStatus.Failure;
                }
                if (next.End) return BehaviorStatus.Failure;

                var b = context.BTExecuteChildNode(next.ChildIndex);
                if (b == BehaviorStatus.Success) return BehaviorStatus.Success;
                if (b == BehaviorStatus.Running)
                {
                    context.SetValue(this, new BTSelectorData()
                    {
                        RunningIndex = i
                    });
                    return BehaviorStatus.Running;
                }
                i=next.ArrayIndex;
            }
        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            var value = context.GetValue(this);
            var index = value.RunningIndex;
            var nRes = ExecuteItem(ref context, ref index, result);
            if (nRes == BehaviorStatus.Running)
            {
                return (false, nRes);
            }
            return (true, nRes);
        }


        private BehaviorStatus ExecuteItem(ref Context context, ref int itemIndex, BehaviorStatus result = BehaviorStatus.None)
        {
            var res = result;
            if (res == BehaviorStatus.Success) return BehaviorStatus.Success;
            if (res == BehaviorStatus.Running)
            {
                context.SetValue(this, new BTSelectorData()
                {
                    RunningIndex = itemIndex
                });
                return BehaviorStatus.Running;
            } // else res == BehaviorStatus.failure to Next
            return ExecuteNext(ref context, itemIndex);;
        }


        public override void OnTick(ref Context context)
        {
            throw new System.NotImplementedException();
        }
    }

    

    //public class BTSelectorAsset : NodeAsset<BTSelector>
    //{

    //}

}

