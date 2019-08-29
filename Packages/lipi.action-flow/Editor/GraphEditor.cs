using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;

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
            object t = null;
            if (t is int a)
            {
                Debug.Log(a);
            }
            Instance = window;

            //Debug.Log(t?.Length);


        }

        public static GraphEditor Instance { get; private set; }

        [SerializeField]
        private GraphAsset _GraphAsset;
        public GraphAsset GraphAsset { private set=> _GraphAsset=value; get=> _GraphAsset; }


        private ActionGraphView _graphView;

        void OnEnable()
        {
            Instance = this;
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

        public List<ISelectable> selection {
            get
            {
                var s = _graphView?.selection;
                if (s == null || s.Count == 0) return null;
                return s;
            }
        }

        private bool playing = false;
        void Update()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                playing = true;
                if (_graphView != null)
                {
                    _graphView.PlayingUpdata();
                    var infoList = RunningGraphAsset.Instance.GetInfoList(GraphAsset);
                    if (infoList == null) return;
                    _graphView.SetRunningEntity(infoList.Infos, infoList.Version);

                }
            }else if(EditorApplication.isPlaying == false)
            {
                if (playing)
                {
                    _graphView.PlayingExit();
                    playing = false;
                }
                //if (_graphView!=null && _graphView.SerializedGraphAsset != null && _graphView.SerializedGraphAsset.hasModifiedProperties)
                //{
                //    _graphView.SerializedGraphAsset.ApplyModifiedProperties();
                //    Debug.Log("dddd");

                //}
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

