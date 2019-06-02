using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ActionFlow
{
    /// <summary>
    /// 在编辑器Play时保存索引关系用于节点状态可视化
    /// </summary>
    public class RunningGraphAsset
    {
        public class Info
        {
            public string Name;
            public ActionStateContainer container;
            public int Index; //ActionStateContainer chunk的索引
            public float InputTime = -1000;

            public NodeCycle GetNodeCycle(int index)
            {
                return container.GetNodeCycle(new ActionStateIndex(Index, index));
            }
        }

        public class InfoList
        {
            public List<Info> Infos;
            public Dictionary<ActionStateIndex, float> InputTime;
            public int Version;

            public InfoList()
            {
                Infos = new List<Info>();
                InputTime = new Dictionary<ActionStateIndex, float>();
            }

        }



        private static RunningGraphAsset _inst;

        public static RunningGraphAsset Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new RunningGraphAsset();
                    _inst.Map = new Dictionary<GraphAsset, InfoList>();
                }
                return _inst;
            }
        }

        private Dictionary<GraphAsset, InfoList> Map;


        public float GetInputTime(GraphAsset graphAsset, ActionStateIndex index)
        {
            if (Map.TryGetValue(graphAsset, out var infoList))
            {
                if (infoList.InputTime.TryGetValue(index, out var t))
                {
                    return t;
                }
            }
            return -1000;
        }

        //private int CurrentFrameCount = -1;
        //private float CurrentFrameFirstTime = 0f;
        public void SetInputTime(GraphAsset graphAsset, ActionStateIndex index)
        {
            if (Map.TryGetValue(graphAsset, out var infoList))
            {
                //var rTime = Time.realtimeSinceStartup;
                //if (CurrentFrameCount != Time.frameCount)
                //{
                //    CurrentFrameCount = Time.frameCount;
                //    CurrentFrameFirstTime = rTime;// Time.realtimeSinceStartup;
                //}
                //Debug.Log($"Time:{CurrentFrameFirstTime}:{Time.realtimeSinceStartup}");
                infoList.InputTime[index] = Time.realtimeSinceStartup;// (rTime - CurrentFrameFirstTime) * 100000f + rTime;
            }
        }



        public void Add(GraphAsset graphAsset, string name, ActionStateContainer container, int index)
        {
            if(Map.TryGetValue(graphAsset, out var list) == false)
            {
                list = new InfoList();
                Map.Add(graphAsset, list);
            }
            list.Version++;
            list.Infos.Add(new Info()
            {
                Name = (name == string.Empty)?"Entity":name,
                container = container,
                Index = index
            });
        }

        public InfoList GetInfoList(GraphAsset graphAsset)
        {
            if (Map.TryGetValue(graphAsset, out var list))
            {
                return list;
            }
            return null;
        }

        public List<Info> GetList(GraphAsset graphAsset)
        {
            if (Map.TryGetValue(graphAsset, out var list))
            {
                return list.Infos;
            }
            return null;
        }

        public Info GetInfo(GraphAsset graphAsset, int index)
        {
            var list = GetList(graphAsset);
            if (list == null) return null;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Index == index) return list[i];
            }
            return null;
        }




    }
}
