using System;
using UnityEngine;

namespace ActionFlow
{
    public struct BTSequenceData
    {
        public int RunningIndex;
    }

    /// <summary>
    /// 顺序执行节点。 执行到返回Failure节点时结束并返回Failure; 全部结束则返回Success
    /// </summary>
    [Serializable]
    [NodeInfo("BT/Sequence")]
    public class BTSequence : StatusNodeBase<BTSequenceData>, IBehaviorCompositeNode
    {
        [HideInInspector]
        public int _i; //无意义,暂时处理无数据类型序列化出错
        
        
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
                if (next.End) return BehaviorStatus.Success;

                var b = context.BTExecuteChildNode(next.ChildIndex);
                if (b == BehaviorStatus.Failure) return BehaviorStatus.Failure;
                if (b == BehaviorStatus.Running)
                {
                    context.SetValue(this, new BTSequenceData()
                    {
                        RunningIndex = next.ArrayIndex
                    });
                    return BehaviorStatus.Running;
                }
                i=next.ArrayIndex;
            }
        }

        public (bool,BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus res)
        {
            var value = context.GetValue(this);
            var index = value.RunningIndex;
            var nres = ExecuteItem(ref context, ref index, res);
            return (nres != BehaviorStatus.Running, nres);
        }


        private BehaviorStatus ExecuteItem(ref Context context, ref int itemIndex, BehaviorStatus result = BehaviorStatus.None)
        {
            var res = result;
            if (res == BehaviorStatus.Failure) return BehaviorStatus.Failure;
            if (res == BehaviorStatus.Running)
            {
                context.SetValue(this, new BTSequenceData
                {
                    RunningIndex = itemIndex
                });
                return BehaviorStatus.Running;
            } // else res == BehaviorStatus.success to 

            return ExecuteNext(ref context, itemIndex);
        }


        public override void OnTick(ref Context context)
        {
            throw new NotImplementedException();
        }
    }

}
