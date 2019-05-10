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
            NativeArray<int> wakingArray = new NativeArray<int>(1000, Allocator.Temp);

            Entities.ForEach((Entity e, ActionGraphAsset asset, ref ActionRunState state) =>
            {
                var stateData = state.State;
                if (stateData.AnyActive == false && stateData.AnyWaking == false) return;

                var nodeList = asset.Asset.RuntimeNodes;
                context.CurrentEntity = e;
                context.TargetEntity = e;
                context.StateData = stateData;
                context.Graph = asset.Asset;
                context.EM = EntityManager;
                context.PostCommand = PostUpdateCommands;


                var (activeCount, wakingCount) = stateData.GetAllActiveOrWakingIndex(ref actives, ref wakingArray);

                for (int i = 0; i < activeCount; i++)
                {
                    context.Index = actives[i];
                    var node = nodeList[actives[i]] as IStatusNode;

                    node.OnTick(ref context);
                }
                for (int i = 0; i < wakingCount; i++)
                {
                    var wakingIndex = wakingArray[i]; 
                    context.Index = wakingIndex;
                    stateData.RemoveNodeCycle(wakingIndex, ActionStateData.NodeCycle.Waking);
#if UNITY_EDITOR
                    if (nodeList[wakingIndex] is ISleepable == false)
                    {
                        throw new System.Exception($"{nodeList[wakingIndex]} 没有实现 ISleepable接口");
                    }
#endif

                    ((ISleepable)nodeList[wakingIndex]).Wake(ref context);

                    //var node = nodeList[wakingIndex] as ISleepable;
                    //node.Wake(ref context);
                }

            });
        }
    }

}