using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System.Reflection;

namespace ActionFlow
{
    public class ActionInspector : EditorWindow
    {
        [MenuItem("Window/Action Inspector")]
        public static void AddWindow()
        {
            ActionInspector window = GetWindow<ActionInspector>();
            window.titleContent.text = "Action Inspector";
            window.Show();
        }

        private StyleSheet _styleSheet;
        private StyleSheet StyleSheet => _styleSheet?? (_styleSheet= Resources.Load<StyleSheet>("ActionInspectorSystem"));


        private List<ISelectable> _CurrentNodes;
        private GraphAsset _CurrentGraph;

        void OnEnable()
        {
            _CurrentGraph = null;
        }
        void Update()
        {
            if (GraphEditor.Instance?.GraphAsset != _CurrentGraph || setSelectedNode() == true)
            {
                _CurrentGraph = GraphEditor.Instance?.GraphAsset;
                DrawUI();
            }
        }

        private bool setSelectedNode()
        {
            var nodes = GraphEditor.Instance?.selection;
            if (nodes == null && _CurrentNodes != null)
            {
                _CurrentNodes = null;
                return true;
            }else if (_CurrentNodes == null && nodes == null)
            {
                return false;
            }else if (_CurrentNodes == null || nodes.Count != _CurrentNodes.Count)
            {
                _set();
                return true;
            } else
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i] != _CurrentNodes[i])
                    {
                        _set();
                        return true;
                    }
                }
                return false;
            }
            void _set()
            {
                _CurrentNodes = new List<ISelectable>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    _CurrentNodes.Add(nodes[i]);
                }
            }

        }




        void DrawUI()
        {
            rootVisualElement.Clear();
            if (rootVisualElement.styleSheets.Contains(StyleSheet) == false)
            {
                rootVisualElement.styleSheets.Add(StyleSheet);
            }
            rootVisualElement.AddToClassList("ActionInspector");


            var title = new Label();
            title.AddToClassList("title");
            title.text = $"{_CurrentGraph?.name}";
            rootVisualElement.Add(title);

            if (_CurrentNodes != null)
            {
                for (int i = 0; i < _CurrentNodes.Count; i++)
                {
                    var node = _CurrentNodes[i] as EditorActionNode;
                    if (node != null && node.NodeAsset != null)
                    {
                        var mSO = new SerializedObject(node.NodeAsset);
                        DrawUI_Node(mSO, node.title);
                    }
                }
            }
        }


        void DrawUI_Node(SerializedObject nodeSO, string name)
        {
            var title = new Label();
            title.text = name;
            title.AddToClassList("title2");
            rootVisualElement.Add(title);

            var nodeVE = new VisualElement();
            nodeVE.AddToClassList("nodeContainer");
            rootVisualElement.Add(nodeVE);

            var sp = nodeSO.GetIterator();
            if (sp.NextVisible(true))
            {
                do
                {
                    if (sp.propertyPath != "m_Script" && sp.propertyPath != "Value")
                    {
                        var be = new PropertyField(sp);
                        be.Bind(nodeSO);
                        var objs = GetAttribute(nodeSO, sp.propertyPath);
                        foreach (var item in objs)
                        {
                            Debug.Log(item);
                        }

                        nodeVE.Add(be);
                    }
                } while (sp.NextVisible(sp.propertyPath == "Value"));
            }
        }

        private object[] GetAttribute(SerializedObject so, string propertyPath)
        {
            var type = so.targetObject.GetType();
            var paths = propertyPath.Split('.');
            FieldInfo fieldInfo = null;
            for (int i = 0; i < paths.Length; i++)
            {
                fieldInfo = type.GetField(paths[i]);
                type = fieldInfo.FieldType;
            }
            return fieldInfo.GetCustomAttributes(false);
        }


        void RefreshView()
        {
            
        }


    }
}
