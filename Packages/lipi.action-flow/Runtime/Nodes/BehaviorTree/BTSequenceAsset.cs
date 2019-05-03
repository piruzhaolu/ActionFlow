using System;
using UnityEngine;

namespace ActionFlow
{

    public struct BTSequenceData
    {
        public int RunningIndex;
    }

    /// <summary>
    /// 顺序执行节点。 执行到返回Failure节点时结束
    /// </summary>
    [Serializable]
    public class BTSequence : StatusNodeBase<BTSequenceData>, IBehaviorNode
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
                    if (b == BehaviorStatus.Failure) return BehaviorStatus.Failure;
                    else if (b == BehaviorStatus.Running)
                    {
                        SetValue(ref context, new BTSequenceData()
                        {
                            RunningIndex = i
                        });
                        return BehaviorStatus.Running;
                    }
                }
                return BehaviorStatus.Success;
            }
            else
            {
                return BehaviorStatus.Failure;
            }
        }

        public (bool,BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus res)
        {
            if (res == BehaviorStatus.Failure) return (true, res);
            var value = GetValue(ref context);

            for (int i = value.RunningIndex+1; i < Childs.Length; i++)
            {
                var b = context.BTNodeOutput(i);
                if (b == BehaviorStatus.Failure) return (true, b);
                else if(b == BehaviorStatus.Running)
                {
                    SetValue(ref context, new BTSequenceData()
                    {
                        RunningIndex = i
                    });
                    return (false, BehaviorStatus.None);
                }
            }
            return (true, BehaviorStatus.Success);
        }

        public override void OnTick(ref Context context)
        {
            throw new NotImplementedException();
        }
    }

    [NodeInfo("BT/Sequence")]
    public class BTSequenceAsset:NodeAsset<BTSequence>
    {

    }
}
