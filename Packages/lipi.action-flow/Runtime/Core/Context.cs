using UnityEngine;
using System.Collections;
using Unity.Entities;

namespace ActionFlow
{
    public struct Context
    {
        /// <summary>
        /// 当前执行Action的Entity
        /// </summary>
        public Entity CurrentEntity { set; get; }

        /// <summary>
        /// 执行作用到的目标
        /// </summary>
        public Entity TargetEntity { set; get; }


        public ActionStateData StateData { set; get; }

        public EntityManager EM { set; get; }


        public int Index { set; get; }

        /// <summary>
        /// 当前节点图
        /// </summary>
        public GraphAsset Graph { set; get; }


        //====================================


        public void Active()
        {
            StateData.SetNodeCycle(Index, ActionStateData.NodeCycle.Active);
        }
        public void Inactive()
        {
            StateData.SetNodeCycle(Index, ActionStateData.NodeCycle.Inactive);
        }
        public void Sleeping()
        {
            StateData.SetNodeCycle(Index, ActionStateData.NodeCycle.Sleeping);
        }

        public void NodeOutput(int outputID = 0)
        {
            var nodeInfo = Graph.NodeInfo[Index];
            if (nodeInfo.Childs == null) return;
            
            for (int i = 0; i < nodeInfo.Childs.Count; i++)
            {
                var child = nodeInfo.Childs[i];
                if (child.FromID == outputID)
                {
                    var tIndex = child.Index;
                    if (Graph.RuntimeNodes[tIndex] is INodeInput nodeInput)
                    {
                        var copyValue = this;
                        copyValue.Index = tIndex;
                        nodeInput.OnInput(ref copyValue);
                    }
                }
            }
        }

        public void NodeOutput<T>(T value, int outputID = 0) where T : struct
        {
            var nodeInfo = Graph.NodeInfo[Index];
            if (nodeInfo.Childs == null) return;

            for (int i = 0; i < nodeInfo.Childs.Count; i++)
            {
                var child = nodeInfo.Childs[i];
                if (child.FromID == outputID)
                {
                    var tIndex = child.Index;
                    if (Graph.RuntimeNodes[tIndex] is INodeInput<T> nodeInput)
                    {
                        var copyValue = this;
                        copyValue.Index = tIndex;
                        nodeInput.OnInput(ref copyValue, value);
                    }
                }
            }
        }



        public BehaviorStatus BTNodeOutput(int arrayIndex = 0, int outputID = 0)
        {
            var nodeInfo = Graph.NodeInfo[Index];
            if (nodeInfo.Childs == null) return BehaviorStatus.None;

            var id = NodeLink.BTIDPre + outputID + arrayIndex * 100;
            for (int i = 0; i < nodeInfo.Childs.Count; i++)
            {
                var child = nodeInfo.Childs[i];
                if (child.FromID == id)
                {
                    var tIndex = child.Index;
                    if (Graph.RuntimeNodes[tIndex] is IBehaviorNode node)
                    {
                        var copyValue = this;
                        copyValue.Index = tIndex;
                        return node.BehaviorInput(ref copyValue);
                    }
                }
            }
            return BehaviorStatus.None;
        }



        /// <summary>
        /// 从其它节点读取参数输入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue"></param>
        /// <param name="index">当前Node在Graph中的索引</param>
        /// <param name="id">为当前输入设定的id</param>
        /// <param name="arrayIndex">如果参数输入在数组中，数组的索引值</param>
        /// <returns></returns>
        public T GetParameter<T>(T defaultValue, int index, int id, int arrayIndex)
        {
            id = id + NodeLink.ParmIDPre + arrayIndex * 100;
            var info = Graph.NodeInfo[index];
            var childs = info.Childs;
            if (childs == null || childs.Count == 0) return defaultValue;

            for (int i = 0; i < childs.Count; i++)
            {
                if (childs[i].FromID == id)
                {
                    var tIndex = childs[i].Index;
                    var tNode = Graph.RuntimeNodes[tIndex];
                    if (tNode is IParameterType<T> n)
                    {
                        return n.GetValue(ref this, tIndex);
                    }
                }
            }
            return defaultValue;
        }

        public T GetParameter<T>(T defaultValue)
        {
            return GetParameter(defaultValue, Index, 0, 0);
        }


        /// <summary>
        /// 取当前节点（只能是行为树）的节点父节点
        /// </summary>
        /// <returns></returns>
        public IBehaviorNode GetParentBehaviorNode()
        {
            var pIndex = Graph.NodeInfo[Index].ParentIndex;
            if (pIndex < Graph.RuntimeNodes.Length)
            {
                if (Graph.RuntimeNodes[pIndex] is IBehaviorNode node)
                {
                    return node;
                }
            }
            return null;
        }

        private void _BehaviorRunningCompleted(ref Context context, BehaviorStatus result)
        {
            var pIndex = Graph.NodeInfo[context.Index].ParentIndex;
            if (pIndex < 0) return;
            if (pIndex < Graph.RuntimeNodes.Length)
            {
                if (Graph.RuntimeNodes[pIndex] is IBehaviorNode node)
                {
                    var childIndex = context.Index;
                    context.Index = pIndex;
                    var (b, res) = node.Completed(ref context, childIndex, result);
                    if (b)
                    {
                        _BehaviorRunningCompleted(ref context, res);
                    }
                }
            }
        }


        public void BehaviorRunningCompleted(BehaviorStatus result)
        {
            var c = this;
            _BehaviorRunningCompleted(ref c, result);
        }


    }

}
