using UnityEngine;
using System.Collections;

namespace ActionFlow
{
    public struct BTParallelSelectorData
    {
        public int Success;
        public int Failure;
        public int Running;
    }

    /// <summary>
    /// 并行控制节点。所有子节点返回failure则返回failure，有一节点返回success则直接返回success
    /// </summary>
    [System.Serializable]
    public class BTParallelSelector : StatusNodeBase<BTParallelSelectorData>, IBehaviorCompositeNode
    {
        [NodeOutputBT(10)]
        public NullStatus[] Childs;


        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var status = new BTParallelSelectorData();

            for (int i = 0; i < Childs.Length; i++)
            {
                var v = context.BTNodeOutput(i);
                if (v == BehaviorStatus.Success)
                {
                    status = new BTParallelSelectorData();
                    context.SetValue(this, status);
                    return BehaviorStatus.Success;
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
                return BehaviorStatus.Failure;
            }
        }

        public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
        {
            var status = context.GetValue(this);
            if (status.Running == 0)
            {// == 0 说明之前已经因为Success返回了
                return (false, BehaviorStatus.None);
            }
            if (result == BehaviorStatus.Success)
            {
                context.SetValue(this, new BTParallelSelectorData());
                return (true, BehaviorStatus.Success);
            } else
            {
                status.Running -= 1;
                context.SetValue(this, status);
                if (status.Running <= 0)
                {
                    return (true, BehaviorStatus.Failure);
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


    [NodeInfo("BT/ParallelSel")]
    public class BBTParallelSelectorAsset : NodeAsset<BTParallelSelector>
    {

    }
}
