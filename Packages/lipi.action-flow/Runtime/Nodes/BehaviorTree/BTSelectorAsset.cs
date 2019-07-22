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
        [HideInActionInspector]
        public NullStatus[] Childs;

        public BehaviorStatus BehaviorInput(ref Context context)
        {
            if (Childs != null)
            {
               // context.SetValue(this, default); 可以不重置，因为不会读出旧数据
                int index = 0;
                return ExecuteItem(ref context, ref index);
                //for (int i = 0; i < Childs.Length; i++)
                //{
                //    var b = context.BTNodeOutput(i);
                //    if (b == BehaviorStatus.Running)
                //    {
                //        context.SetValue(this, new BTSelectorData()
                //        {
                //            RunningIndex = i
                //        });
                //        return BehaviorStatus.Running;
                //    } else if (b == BehaviorStatus.Success)
                //    {
                //        return BehaviorStatus.Success;
                //    }
                //}
            }
            return BehaviorStatus.Failure;
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


            //if (result == BehaviorStatus.Success) return (true, result);
            //var val = context.GetValue(this);

            //for (int i = val.RunningIndex+1; i < Childs.Length; i++)
            //{
            //    var b = context.BTNodeOutput(i);
            //    if (b == BehaviorStatus.Success) return (true, b);
            //    else if(b == BehaviorStatus.Running)
            //    {
            //        context.SetValue(this, new BTSelectorData()
            //        {
            //            RunningIndex = i
            //        });
            //        return (false, BehaviorStatus.None);
            //    }
            //}

            //return (true, BehaviorStatus.Failure);
        }


        private BehaviorStatus ExecuteItem(ref Context context, ref int itemIndex, BehaviorStatus result = BehaviorStatus.None)
        {
            BehaviorStatus res = result;
            if (res == BehaviorStatus.None)
            {
                res = context.BTNodeOutput(itemIndex);
            }
            if (res == BehaviorStatus.Success) return BehaviorStatus.Success;
            else if (res == BehaviorStatus.Running)
            {
                context.SetValue(this, new BTSelectorData()
                {
                    RunningIndex = itemIndex
                });
                return BehaviorStatus.Running;
            } // else res == BehaviorStatus.failure to Next
            itemIndex += 1;
            if (itemIndex >= Childs.Length)
            {
                return BehaviorStatus.Failure;
            }
            else
            {
                return ExecuteItem(ref context, ref itemIndex);
            }
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

