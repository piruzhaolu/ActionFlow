using System;
using System.Collections.Generic;
using UnityEngine;


namespace ActionFlow
{
    public class ActionFlowGraphAsset: ScriptableObject
    {
        [Serializable]
        public struct GrapChildItem
        {
            public int CurrentID;
            public int TargetIndex;
            public int TargetID;
            public PortMode PortMode;
        }

        //IBehaviorNode的包装结构
        public struct BehaviorNodeStruct
        {
            public IBehaviorNode Node;
            public int Index; //节点所在的索引
        }


        [Serializable]
        public class GraphItem
        {
            public List<GrapChildItem> Childs;
        }


        public int Entry;

        public List<ScriptableObject> NodeAssets;

        public List<GraphItem> Graph;

        public ScriptableObject EntryNode
        {
            get
            {
                return NodeAssets[Entry];
            }
        }

        private Dictionary<int, BehaviorNodeStruct[]> AllBTNodeChild;

        public BehaviorNodeStruct[] GetBTNodeChild(int index)
        {
            if (AllBTNodeChild == null)
            {
                AllBTNodeChild = new Dictionary<int, BehaviorNodeStruct[]>();
                for (int i = 0; i < Graph.Count; i++)
                {
                    var childs = Graph[i].Childs;
                    var btCount = 0;
                    for (int j = 0; j < childs.Count; j++)
                    {
                        if (childs[j].PortMode == PortMode.BT)
                        {
                            btCount++;
                        }
                    }
                    if (btCount > 0)
                    {
                        var nodes = new BehaviorNodeStruct[btCount];
                        for (int j = 0; j < childs.Count; j++)
                        {
                            var nodesIndex = 0;
                            if (childs[j].PortMode == PortMode.BT)
                            {
                                var tIndex = childs[j].TargetIndex;
                                var behaviorNode = NodeAssets[tIndex] as IBehaviorNode;
                                nodes[nodesIndex] = new BehaviorNodeStruct()
                                {
                                    Node = behaviorNode,
                                    Index = tIndex
                                };
                                nodesIndex++;
                            }
                        }
                        AllBTNodeChild.Add(i, nodes);
                    }
                }
            }
            if (AllBTNodeChild.TryGetValue(index, out var value)) return value;

            return null;
        }


        private int[] AllBTNodeParent;
    
        public int GetBTNodeParent(int index)
        {
            if (AllBTNodeParent == null)
            {
                AllBTNodeParent = new int[Graph.Count];
                for (int i = 0; i < AllBTNodeParent.Length; i++)
                {
                    AllBTNodeParent[i] = -1;
                }
                for (int i = 0; i < Graph.Count; i++)
                {
                    var childs = Graph[i].Childs;
                    if (childs != null)
                    {
                        for (int j = 0; j < childs.Count; j++)
                        {
                            if (childs[j].PortMode == PortMode.BT)
                            {
                                AllBTNodeParent[childs[j].TargetIndex] = i;
                            }
                        }
                    }
                }

            }
            return AllBTNodeParent[index];
        }


        #region 编辑器数据

        [System.Serializable]
        public class NodeInfo
        {
            public Vector2 pos;

            public bool expand;
        }

        //[HideInInspector]
        public List<NodeInfo> NodeInfoList = new List<NodeInfo>();


        //[HideInInspector]
        public Vector3 Zoom = Vector3.one;

        //[HideInInspector]
        public Vector3 viewPos = Vector3.zero;


        #endregion
    }
}
