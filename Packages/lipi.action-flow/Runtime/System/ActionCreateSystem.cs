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
                None = new ComponentType[] { typeof(ActionGraphCreated) },
                All = new ComponentType[] { ComponentType.ReadOnly<ActionGraphAsset>() }
            });
        }


        protected override void OnUpdate()
        {

            Entities.With(m_Group).ForEach((Entity e, ActionGraphAsset asset) =>
            {
                var stateData = ActionStateData.Create(asset.Asset);
                stateData.SetNodeCycle(asset.Asset.Entry, ActionStateData.NodeCycle.Active);
                PostUpdateCommands.AddComponent(e, new ActionRunState()
                {
                    State = stateData
                });
                PostUpdateCommands.AddComponent(e, new ActionGraphCreated() { });
            });
        }
    }
}