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
    /// 并行控制节点,会同时调用子节点。所有子节点返回failure则返回failure，有一节点返回success则直接返回success
    /// </summary>
    [System.Serializable]
    [NodeInfo("BT/ParallelSel")]
    public class BTParallelSelector : StatusNodeBase<BTParallelSelectorData>, IBehaviorCompositeNode
    {
        [HideInInspector]
        public int _i; //无意义,暂时处理无数据类型序列化出错
        
        [NodeOutputBT(5)]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            var status = new BTParallelSelectorData();

            for (var i = 0; i < 5; i++)
            {
                var v = context.BTNodeOutput(i);
                switch (v)
                {
                    case BehaviorStatus.Success:
                        status.Success += 1;
                        break;
                    case BehaviorStatus.Failure:
                        status.Failure += 1;
                        break;
                    case BehaviorStatus.Running:
                        status.Running += 1;
                        break;
                }
            }

            if (status.Success >= 0)
            {
                context.SetValue(this, default);
                return BehaviorStatus.Success;
            }
            
            context.SetValue(this, status);
            return status.Running > 0 ? BehaviorStatus.Running : BehaviorStatus.Failure;
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
                context.SetValue(this, default);
                return (true, BehaviorStatus.Success);
            }
            status.Running -= 1;
            context.SetValue(this, status);
            return status.Running <= 0 ? (true, BehaviorStatus.Failure) : (false, BehaviorStatus.Running);

        }

        public override void OnTick(ref Context context)
        {
            throw new System.NotImplementedException();
        }

    }


    //[NodeInfo("BT/ParallelSel")]
    //public class BBTParallelSelectorAsset : NodeAsset<BTParallelSelector>
    //{

    //}
}
