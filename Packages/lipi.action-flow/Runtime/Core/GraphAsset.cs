using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ActionFlow
{

    [CreateAssetMenu(menuName = "GraphAsset")]
    public class GraphAsset : ScriptableObject
    {

        #region 运行时数据
        public List<ScriptableObject> Nodes = new List<ScriptableObject>();

        //public List<INode> Nodes = new List<INode>(); //2019.3

        public int Entry;

        public List<GraphNodeInfo> NodeInfo = new List<GraphNodeInfo>();


        private INode[] _RuntimeNodes = null;

        /// <summary>
        /// 
        /// </summary>
        public INode[] RuntimeNodes
        {
            get
            {
                if (_RuntimeNodes == null)
                {
                    _RuntimeNodes = new INode[Nodes.Count];
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        if (Nodes[i] != null) _RuntimeNodes[i] = (Nodes[i] as INodeAsset)?.GetValue();
                    }
                }
                if (_RuntimeNodes.Length == 0)
                {
                    throw new System.Exception("node length error");
                }
                return _RuntimeNodes;
            }
        }

        #endregion


        #region 编辑器数据
        [System.NonSerialized]
        public string guid;

        public List<GraphNodeEditorInfo> NodeEditorInfo = new List<GraphNodeEditorInfo>();

        public Vector2 ViewPosition = Vector2.zero;
        public Vector3 ViewScale = Vector3.one;
        #endregion



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

        public void Remove(ScriptableObject so)
        {
#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(so);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this));
#endif
        }

    }


    //=================================

    [System.Serializable]
    public struct GraphNodeEditorInfo
    {
        public int Index;
        public Vector2 Pos;
    }

    [System.Serializable]
    public class GraphNodeInfo
    {
        public int CurrentIndex;
        public List<NodeLink> Childs;
        public int ParentIndex = -1;
    }

    [System.Serializable]
    public struct NodeLink
    {
        public static readonly int ParmIDPre = 10000;
        public static readonly int BTIDPre = 100000;

        public int FromID;
        public int Index;
        public int ToID;
    }

    

}
