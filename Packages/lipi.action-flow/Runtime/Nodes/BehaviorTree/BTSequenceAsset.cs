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
    public class BTSequence : StatusNodeBase<BTSequenceData>, IBehaviorCompositeNode
    {
        [NodeOutputBT(10)]
        public NullStatus[] Childs;

        public BehaviorStatus BehaviorInput(ref Context context)
        {
            if (Childs != null)
            {
                //context.SetValue(this, default); 可以不重置，因为不会读出旧数据
                int index = 0;
                return ExecuteItem(ref context, ref index);
                //for (int i = 0; i < Childs.Length; i++)
                //{
                //    var b = context.BTNodeOutput(i);
                //    if (b == BehaviorStatus.Failure) return BehaviorStatus.Failure;
                //    else if (b == BehaviorStatus.Running)
                //    {
                //        context.SetValue(this, new BTSequenceData()
                //        {
                //            RunningIndex = i
                //        });
                //        return BehaviorStatus.Running;
                //    }
                //}
                //return BehaviorStatus.Success;
            }
            else
            {
                return BehaviorStatus.Failure;
            }
        }

        public (bool,BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus res)
        {
            var value = context.GetValue(this);
            var index = value.RunningIndex;
            var nres = ExecuteItem(ref context, ref index, res);
            return (nres != BehaviorStatus.Running, nres);

            //if (res == BehaviorStatus.Failure) return (true, res);
            //var value = context.GetValue(this);

            //for (int i = value.RunningIndex+1; i < Childs.Length; i++)
            //{
            //    var b = context.BTNodeOutput(i);
            //    if (b == BehaviorStatus.Failure) return (true, b);
            //    else if(b == BehaviorStatus.Running)
            //    {
            //        context.SetValue(this, new BTSequenceData()
            //        {
            //            RunningIndex = i
            //        });
            //        return (false, BehaviorStatus.None);
            //    }
            //}
            //return (true, BehaviorStatus.Success);
        }


        private BehaviorStatus ExecuteItem(ref Context context, ref int itemIndex, BehaviorStatus result = BehaviorStatus.None)
        {
            BehaviorStatus res = result;
            if (res == BehaviorStatus.None)
            {
                res = context.BTNodeOutput(itemIndex);
            }
            if (res == BehaviorStatus.Failure) return BehaviorStatus.Failure;
            else if (res == BehaviorStatus.Running)
            {
                context.SetValue(this, new BTSequenceData()
                {
                    RunningIndex = itemIndex
                });
                return BehaviorStatus.Running;
            } // else res == BehaviorStatus.success to Next
            itemIndex += 1;
            if (itemIndex >= Childs.Length)
            {
                return BehaviorStatus.Success;
            } else
            {
                return ExecuteItem(ref context, ref itemIndex);
            }
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
