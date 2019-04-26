using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ActionFlow
{


    public class EditorActionNode : Node
    {

        public EditorActionNode(ScriptableObject nodeAsset, GraphNodeInfo nodeInfo, GraphNodeEditorInfo nodeEditorInfo, int index)
        {

            NodeAsset = nodeAsset;
            NodeInfo = nodeInfo;
            NodeEditorInfo = nodeEditorInfo;
            Index = index;

            var infos = nodeAsset.GetType().GetCustomAttributes(typeof(NodeInfoAttribute), false);
            NodeInfoAttribute info;

            if (infos != null && infos.Length > 0)
            {
                info = (NodeInfoAttribute)infos[0];
                title = info.Name;
            }
            else
            {
                title = nodeAsset.name;
            }

            var drawer = DefaltEditorNodeDraw.GetEditor(nodeAsset.GetType());
            drawer.Create(this, nodeAsset);
        }


        public ScriptableObject NodeAsset { private set; get; }
        public GraphNodeInfo NodeInfo { private set; get; }
        public GraphNodeEditorInfo NodeEditorInfo { private set; get; }
        public int Index { private set; get; }



        public Port GetPort(int id, NodeTypeInfo.IOMode mode)
        {
            var list = this.Query<Port>().ToList();
            foreach (var item in list)
            {
                var info = (NodeTypeInfo.IOInfo)item.source;
                if (info != null && info.ID == id && info.Mode == mode)
                {
                    return item;
                }

            }
            return null;
        }

    }
}

