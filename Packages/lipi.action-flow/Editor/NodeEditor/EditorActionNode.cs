using UnityEngine;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ActionFlow
{


    public class EditorActionNode : Node
    {
        public delegate void EdgeChange();

        public EditorActionNode(ScriptableObject nodeAsset, GraphNodeInfo nodeInfo, GraphNodeEditorInfo nodeEditorInfo, int index)
        {

            NodeAsset = nodeAsset;
            NodeInfo = nodeInfo;
            NodeEditorInfo = nodeEditorInfo;
            Index = index;

            RunningMarker = new VisualElement();
            RunningMarker.style.width = 4f;
            titleContainer.hierarchy.Insert(0, RunningMarker);

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

            var drawer = DefaultEditorNodeDraw.GetEditor(nodeAsset.GetType());
            drawer.Create(this, nodeAsset);
        }

        private VisualElement RunningMarker;

        private bool _running = false;
        public bool Running
        {
            set
            {
                if (value == _running) return;
                _running = value;
                if (value == true)
                {
                    RunningMarker.style.backgroundColor = new StyleColor(new Color(0.446f, 0.811f, 0.344f, 1f));
                    //this.Q("title").style.backgroundColor = new StyleColor(new Color(0.669f, 0.4f, 0.164f,0.803f));
                }else
                {
                    //this.Q("title").style.backgroundColor = new StyleColor(new Color(0.247f, 0.247f, 0.247f,0.803f));
                    RunningMarker.style.backgroundColor = new StyleColor(new Color(0.446f, 0.811f, 0.344f, 0f));
                }
            }
        }

        public void InputTween(float t)
        {
            var colorIn = new Color(0.669f, 0.4f, 0.164f, 0.803f); //
            var colorOut = new Color(0.247f, 0.247f, 0.247f, 0.803f); //默认色
            var v = Color.Lerp(colorIn, colorOut,t);
            titleContainer.style.backgroundColor = new StyleColor(v);
        }




        public ScriptableObject NodeAsset { private set; get; }
        public GraphNodeInfo NodeInfo { private set; get; }
        public GraphNodeEditorInfo NodeEditorInfo { private set; get; }
        public int Index { private set; get; }


        public event EdgeChange EdgeChangeHandler;


        public void EdgeAddOrRemove()
        {
            EdgeChangeHandler?.Invoke();
        }

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

