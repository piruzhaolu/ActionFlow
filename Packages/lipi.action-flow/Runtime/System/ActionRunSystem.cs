using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

namespace ActionFlow
{
    [UpdateAfter(typeof(ActionCreateSystem))]
    public class ActionRunSystem : ComponentSystem
    {


        protected override void OnUpdate()
        {
            var context = new Context();
            NativeArray<int> actives = new NativeArray<int>(1000, Allocator.Temp);

            Entities.ForEach((Entity e, ActionGraphAsset asset, ref ActionRunState state) =>
            {
                var stateData = state.State;
                if (stateData.AllInactive || stateData.AllSleeping) return;

                var nodeList = asset.Asset.RuntimeNodes;
                context.CurrentEntity = e;
                context.TargetEntity = e;
                context.StateData = stateData;
                context.Graph = asset.Asset;
                context.EM = EntityManager;
                context.PostCommand = PostUpdateCommands;


                var count = stateData.GetAllActiveOrSleepingIndex(ref actives);

                for (int i = 0; i < count; i++)
                {
                    context.Index = actives[i];
                    var node = nodeList[actives[i]] as IStatusNode;

                    node.OnTick(ref context);
                }


            });
        }
    }

}