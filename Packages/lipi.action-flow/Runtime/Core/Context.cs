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


        public void NodeOutput<T>(T value, int outputID = 0) where T:struct
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


        public T GetParameter<T>(int index, int id, T defaultValue)
        {
            id = id + NodeLink.ParmIDPre;
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
            return GetParameter(Index, 0, defaultValue);
        }
        public T GetParameter<T>(int id, T defaultValue)
        {
            return GetParameter(Index, id, defaultValue);
        }





    }

}
