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
        public List<ScriptableObject> Nodes = new List<ScriptableObject>();

        public int Entry;


        public List<GraphNodeInfo> NodeInfo = new List<GraphNodeInfo>();




        [System.NonSerialized]
        public string guid;

        public List<GraphNodeEditorInfo> NodeEditorInfo = new List<GraphNodeEditorInfo>();

        public Vector2 ViewPosition = Vector2.zero;
        public Vector3 ViewScale = Vector3.one;


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
        public int ParentIndex;
    }

    [System.Serializable]
    public struct NodeLink
    {
        public int FromID;
        public int Index;
        public int ToID;
    }

    

}
