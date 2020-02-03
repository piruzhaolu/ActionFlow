using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Graphs;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ActionFlow
{

    [CreateAssetMenu(menuName = "GraphAsset")]
    public class GraphAsset : ScriptableObject
    {

        #region 运行时数据
        [Obsolete]
        public List<ScriptableObject> Nodes = new List<ScriptableObject>();

        [SerializeReference]
        public List<INode> m_Nodes = new List<INode>();

        public int Entry;

        public List<GraphNodeInfo> NodeInfo = new List<GraphNodeInfo>();


        private INode[] _runtimeNodes ;

        /// <summary>
        /// 
        /// </summary>
        public INode[] RuntimeNodes
        {
            get
            {
                if (_runtimeNodes == null)
                {
                    _runtimeNodes = new INode[m_Nodes.Count];
                    for (int i = 0; i < m_Nodes.Count; i++)
                    {
                        if (m_Nodes[i] != null) _runtimeNodes[i] = m_Nodes[i];// (m_Nodes[i] as INodeAsset)?.GetValue();
                    }
                }
                if (_runtimeNodes.Length == 0)
                {
                    throw new Exception("node length error");
                }
                return _runtimeNodes;
            }
        }

        #endregion


        #region 编辑器数据
        [NonSerialized]
        public string Guid;

        public List<GraphNodeEditorInfo> NodeEditorInfo = new List<GraphNodeEditorInfo>();

        public Vector2 ViewPosition = Vector2.zero;
        public Vector3 ViewScale = Vector3.one;
        #endregion


        public int Add(INode node, Vector2 pos)
        {
            m_Nodes.Add(node);
            var index = m_Nodes.Count - 1;
            NodeInfo.Add(new GraphNodeInfo()
            {
                CurrentIndex = index
            });
            NodeEditorInfo.Add(new GraphNodeEditorInfo()
            {
                Index = index,
                Pos = pos
            });
            return index;
        }

        public void Remove(INode node)
        {
            var index = m_Nodes.IndexOf(node);
            if (index != -1) m_Nodes[index] = null;
        }

        [Obsolete]
        public int Add(ScriptableObject so, Vector2 pos)
        {
            Nodes.Add(so);
            var index = Nodes.Count - 1;
            NodeInfo.Add(new GraphNodeInfo()
            {
                CurrentIndex = index
            });
            NodeEditorInfo.Add(new GraphNodeEditorInfo()
            {
                Index = index,
                Pos = pos
            });
            
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(so,this);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
            return index;
        }

        [Obsolete]
        public void Remove(ScriptableObject so)
        {
#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(so);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }

    }


    //=================================

    [Serializable]
    public struct GraphNodeEditorInfo
    {
        public int Index;
        public Vector2 Pos;
    }

    [Serializable]
    public class GraphNodeInfo
    {
        public int CurrentIndex;
        public List<NodeLink> Childs;
        public int ParentIndex = -1;
    }

    [Serializable]
    public struct NodeLink
    {
        public static readonly int ParmIDPre = 10000;
        public static readonly int BTIDPre = 100000;

        public int FromID;
        public int Index;
        public int ToID;
    }

    

}
