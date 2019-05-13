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
                All = new ComponentType[] { ComponentType.ReadOnly<ActionGraphAsset>() }
            });
        }


        protected override void OnUpdate()
        {

            Entities.With(m_Group).ForEach((Entity e, ActionGraphAsset asset) =>
            {
                ref ActionStateContainer container = ref ActionStateMapToAsset.Instance.GetContainer(asset.Asset);
                var index = container.AddChunk();
                //var stateData = ActionStateData.Create(asset.Asset);
                //stateData.SetNodeCycle(asset.Asset.Entry, NodeCycle.Active);
                //TODO: 把入口入在启动逻辑中，而不是通过设置为active来处理
                container.GetStateForEntity(index).SetNodeCycle(asset.Asset.Entry, NodeCycle.Active);
                PostUpdateCommands.AddComponent(e, new ActionRunState()
                {
                    InstanceID = asset.Asset.GetInstanceID(),
                    Index = index
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