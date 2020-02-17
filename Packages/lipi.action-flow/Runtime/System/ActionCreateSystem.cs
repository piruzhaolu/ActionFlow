using UnityEngine;
using System.Collections;
using Unity.Entities;


namespace ActionFlow
{
    public class ActionCreateSystem : ComponentSystem
    {


        EntityQuery m_Group;

        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[] { typeof(ActionRunState) },
                All = new[] { ComponentType.ReadOnly<ActionGraphAsset>() }
            });
        }


        protected override void OnUpdate()
        {

            Entities.With(m_Group).ForEach((Entity e, ActionGraphAsset asset) =>
            {
                ref ActionStateContainer container = ref ActionStateMapToAsset.Instance.GetContainer(asset.Asset);
                var index = container.AddChunk();
#if UNITY_EDITOR
                var eName = EntityManager.GetName(e);
                RunningGraphAsset.Instance.Add(asset.Asset, eName, container, index);
#endif
                //var stateData = ActionStateData.Create(asset.Asset);
                //stateData.SetNodeCycle(asset.Asset.Entry, NodeCycle.Active);
                //TODO: 把入口入在启动逻辑中，而不是通过设置为active来处理
                var entity = PostUpdateCommands.CreateEntity();

                //var entity = EntityManager.CreateEntity();
                var stateIndex = new ActionStateIndex(index, asset.Asset.Entry);
                container.SetNodeCycle(stateIndex, NodeCycle.Active);
                PostUpdateCommands.AddComponent(e, new ActionRunState()
                {
                    InstanceID = asset.Asset.GetInstanceID(),
                    ChunkIndex = index
                });
                //PostUpdateCommands.AddComponent(e, new ActionGraphCreated() { });
            });
        }

        protected override void OnDestroy()
        {
            ActionStateMapToAsset.Instance.Dispose();
        }
    }
}