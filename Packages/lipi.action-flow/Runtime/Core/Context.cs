using UnityEngine;
using System.Collections;
using Unity.Entities;

#pragma warning disable IDE0060 // 删除未使用的参数
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


        public ActionStateForEntity StateData { set; get; }

        public EntityManager EM { set; get; }

        public EntityCommandBuffer PostCommand;


        public int Index { set; get; }

        /// <summary>
        /// 当前节点图
        /// </summary>
        public GraphAsset Graph { set; get; }



        //====================================

        public void Active(IStatusNode node)
        {
            StateData.SetNodeCycle(Index, NodeCycle.Active);
        }
        public void Inactive(IStatusNode node)
        {
            StateData.SetNodeCycle(Index, NodeCycle.Inactive);
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


        #region get set Value
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


        public T GetValue<T>(IStatusNode<T> node)  where T: struct
        {
            return StateData.GetValue<T>(Index);
        }

        /// <summary>
        /// node为当前处理的节点 如SetValue(this,value)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void SetValue<T>(IStatusNode<T> node, T value) where T: struct

        {
            StateData.SetValue(Index, value);
        }

        #endregion


        #region sleep

        /// <summary>
        /// 将node的运行转移到system,并让node进入睡眠状态. 
        /// component 是 System与ActionFlow的关联, 当被添加到entity system系统执行, system将它删除时node会被唤醒
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        public void TransferToSystemAndSleep<T>(ISleepable node, T component) where T : struct, IComponentData
        {
            DynamicBuffer<NodeSleeping> buffers;
            if (EM.HasComponent<NodeSleeping>(TargetEntity))
            {
                buffers = EM.GetBuffer<NodeSleeping>(TargetEntity);
            }
            else
            {
                buffers = PostCommand.AddBuffer<NodeSleeping>(TargetEntity);
            }
            buffers.Add(new NodeSleeping()
            {
                Entity = CurrentEntity,
                NodeIndex = Index,
                ComponentType = ComponentType.ReadWrite<T>()
            });
            PostCommand.AddComponent(TargetEntity, component);
            StateData.SetNodeCycle(Index, NodeCycle.Sleeping);
        }


        public void SetWakeTimerAndSleep(ISleepable node, float t)
        {
            DynamicBuffer<NodeTimer> buffers;
            if (EM.HasComponent<NodeTimer>(CurrentEntity))
            {
                buffers = EM.GetBuffer<NodeTimer>(CurrentEntity);
            }
            else
            {
                buffers = PostCommand.AddBuffer<NodeTimer>(CurrentEntity);
            }
            buffers.Add(new NodeTimer()
            {
                Time = t,
                NodeIndex = Index
            });
            StateData.SetNodeCycle(Index, NodeCycle.Sleeping);
        }
        #endregion


        #region 父节点操作和RunningCompleted冒泡
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
                if (Graph.RuntimeNodes[pIndex] is IBehaviorCompositeNode node)
                {
                    var childIndex = context.Index;
                    context.Index = pIndex;
                    var (b, res) = node.Completed(ref context, childIndex, result);
                    if (b)
                    { //向 parent node 递归
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
        #endregion

    }

}
#pragma warning restore IDE0060 // 删除未使用的参数