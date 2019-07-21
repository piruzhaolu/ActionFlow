using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


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


        private EditorActionNode _CurrentNode;
        private GraphAsset _CurrentGraph;

        void OnEnable()
        {
            _CurrentGraph = null;
        }
        void Update()
        {
            if (GraphEditor.Instance?.GraphAsset != _CurrentGraph || _CurrentNode != GraphEditor.Instance?.selection)
            {
                _CurrentGraph = GraphEditor.Instance?.GraphAsset;
                _CurrentNode = GraphEditor.Instance?.selection;
                DrawUI();
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


            var nodeName = (_CurrentNode == null) ? _CurrentGraph?.name : _CurrentNode.title;
            var title = new Label();
            title.name = "title";
            title.text = $"{nodeName} - {_CurrentGraph?.name}";
            rootVisualElement.Add(title);


        }


        void RefreshView()
        {
            
        }


    }
}
