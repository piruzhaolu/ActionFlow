using UnityEngine;
using System.Collections.Generic;
using System;

namespace ActionFlow
{


    public class ActionStateMapToAsset:IDisposable
    {
        private Dictionary<int, ActionStateContainer[]> _map;

        private static ActionStateMapToAsset _inst;

        public static ActionStateMapToAsset Instance
        {
            get
            {
                if (_inst == null)
                {
                    _inst = new ActionStateMapToAsset();
                    _inst._map = new Dictionary<int, ActionStateContainer[]>();
                }
                return _inst;
            }
        }

        public ref ActionStateContainer GetContainer(GraphAsset graph)
        {
            if (_map.TryGetValue(graph.GetInstanceID(), out var actionStateContainer))
            {
                return ref actionStateContainer[0];
            }
            else
            {
                ActionStateContainer[] asc = new ActionStateContainer[1];
                asc[0] = ActionStateContainer.Create(graph);
                _map.Add(graph.GetInstanceID(), asc);
                return ref asc[0];
            }
        }


        public int CreateContainer(GraphAsset graph)
        {
            var id = graph.GetInstanceID();
            if (_map.ContainsKey(id)) return id;
           
            ActionStateContainer[] asc = new ActionStateContainer[1];
            asc[0] = ActionStateContainer.Create(graph);
            _map.Add(id, asc);
            return id;
        }


        public ref ActionStateContainer GetContainer(int instanceID)
        {
            if (_map.TryGetValue(instanceID, out var actionStateContainer))
            {
                return ref actionStateContainer[0];
            }
            else
            {
                throw new System.Exception("对应的GraphAsset没有被创建");
            }
        }

        public void Dispose()
        {
            foreach (var kv in _map)
            {
                kv.Value[0].Dispose();
            }
            _map = null;
        }
    }

}
