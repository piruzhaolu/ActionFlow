using UnityEngine;
using System;

namespace ActionFlow
{
    public struct BTSelectorData
    {
        public int RunningIndex;
    }

    /// <summary>
    /// 选择节点。 执行到返回Success节点时结束并返回Success
    /// </summary>
    [Serializable]
    public class BTSelector : StatusNodeBase<BTSelectorData>, IBehaviorCompositeNode
    {

        [NodeOutputBT(10)]
        public NullStatus[] Childs;

        public BehaviorStatus BehaviorInput(ref Context context)
        {
            if (Childs != null)
            {
                for (int i = 0; i < Childs.Length; i++)
                {
                    var b = context.BTNodeOutput(i);
                    if (b == BehaviorStatus.Running)
                    {
                        context.SetValue(this, new BTSelectorData()
                        {
                            RunningIndex = i
                        });
                        return BehaviorStatus.Running;
                    } else if (b == BehaviorStatus.Success)
                    {
                        return BehaviorStatus.Success;
                    }
                }
            }
            return BehaviorStatus.Failure;
        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            if (result == BehaviorStatus.Success) return (true, result);
            var val = context.GetValue(this);

            for (int i = val.RunningIndex+1; i < Childs.Length; i++)
            {
                var b = context.BTNodeOutput(i);
                if (b == BehaviorStatus.Success) return (true, b);
                else if(b == BehaviorStatus.Running)
                {
                    context.SetValue(this, new BTSelectorData()
                    {
                        RunningIndex = i
                    });
                    return (false, BehaviorStatus.None);
                }
            }

            return (true, BehaviorStatus.Failure);
        }

        public override void OnTick(ref Context context)
        {
            throw new System.NotImplementedException();
        }
    }

    [NodeInfo("BT/Selector")]

    public class BTSelectorAsset : NodeAsset<BTSelector>
    {

    }

}

