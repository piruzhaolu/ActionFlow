using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace ActionFlow
{
    // ActionFlow 编辑器
    public class GraphEditor : EditorWindow
    {
        public static void AddWindow(GraphAsset asset) //ActionScriptGraphAsset graphAsset
        {
            GraphEditor window = (GraphEditor)GetWindow(typeof(GraphEditor));
            window.titleContent.text = "ActionFlow Graph Editor";
            window.Show();

            window.GraphAsset = asset;
            if (window._graphView != null)
            {
                window._graphView.Show(asset);
            }

            window.Repaint();
           // window.RefreshView();
            

        }

        public GraphAsset GraphAsset;
        private ActionGraphView _graphView;

        void OnEnable()
        {
           RefreshView();
        }

        void RefreshView()
        {
            if (_graphView == null)
            {
                _graphView = new ActionGraphView(this);
                rootVisualElement.Add(_graphView);
            }
            if (GraphAsset != null)
            {
                _graphView.Show(GraphAsset);
            }

        }




        [OnOpenAsset()]
        public static bool OpenWindow(int insID, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(insID);
            System.Type type = obj.GetType();
            if (type == typeof(GraphAsset))
            {
                GraphEditor.AddWindow(obj as GraphAsset);
                return true;
            }
            return false;
        }


    }

}

