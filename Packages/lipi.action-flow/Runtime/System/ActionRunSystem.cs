using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Profiling;

namespace ActionFlow
{
    [UpdateAfter(typeof(ActionCreateSystem))]
    public class ActionRunSystem : ComponentSystem
    {

        private EntityQuery m_Group;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(ComponentType.ReadOnly<ActionGraphAsset>(),ComponentType.ReadOnly<ActionRunState>());

            
        }


        struct ActionRunJob : IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                throw new System.NotImplementedException();
            }
        }

        protected override void OnUpdate()
        {
            var chunkArray = m_Group.CreateArchetypeChunkArray(Allocator.TempJob);
            var sComponentType = GetArchetypeChunkSharedComponentType<ActionGraphAsset>();
            var stateComponentType = GetArchetypeChunkComponentType<ActionRunState>(true);
            var entityType = GetArchetypeChunkEntityType();
            

            NativeArray<int> actives = new NativeArray<int>(1000, Allocator.Temp);
            NativeArray<int> wakingArray = new NativeArray<int>(1000, Allocator.Temp);
            
            var context = new Context();
           
            for (int i = 0; i < chunkArray.Length; i++)
            {
                var asset = chunkArray[i].GetSharedComponentData(sComponentType, EntityManager);
                ref ActionStateContainer container = ref ActionStateMapToAsset.Instance.GetContainer(asset.Asset.GetInstanceID());
                var runStateArray = chunkArray[i].GetNativeArray(stateComponentType);
                var entityArray = chunkArray[i].GetNativeArray(entityType);
                var nodeList = asset.Asset.RuntimeNodes;

                context.Graph = asset.Asset;
                context.EntityManager = EntityManager;
                context.PostUpdateCommands = PostUpdateCommands;
                context.StateData = container;
                for (int j = 0; j < runStateArray.Length; j++)
                {
                    var chunkIndex = runStateArray[j].ChunkIndex;
                    if (container.AnyActive(chunkIndex) == false && container.AnyWaking(chunkIndex) == false) continue;
                    context.CurrentEntity = entityArray[j];
                    context.TargetEntity = entityArray[j];


                    //Profiler.BeginSample("lipi_C");
                    //TODO: 需要优化，节点越多性能越差
                    var (activeCount, wakingCount) = container.GetAllActiveOrWakingIndex(chunkIndex, ref actives, ref wakingArray);
                    //Profiler.EndSample();

                    
                    for (int k = 0; k < activeCount; k++)
                    {
                        context.Index = new ActionStateIndex(chunkIndex, actives[k]);
                        var node = nodeList[actives[k]] as IStatusNode;
                        //Profiler.BeginSample("lipi_V");
                        node.OnTick(ref context); //TODO:需要优化 
                        //Profiler.EndSample();
                    }
                   
                    for (int k = 0; k < wakingCount; k++)
                    {
                        var wakingIndex = wakingArray[k];
                        context.Index = new ActionStateIndex(chunkIndex, wakingIndex);
                        container.RemoveNodeCycle(context.Index, NodeCycle.Waking);
                        ((ISleepable)nodeList[wakingIndex]).Wake(ref context);
                    }
                }

            }
            chunkArray.Dispose();
            
            //return inputDeps;
        }
    }

}



//[UpdateAfter(typeof(ActionCreateSystem))]
//    [DisableAutoCreation]
//    public class ActionRunSystem : ComponentSystem
//    {


//        protected override void OnUpdate()
//        {
//            var context = new Context();
//            NativeArray<int> actives = new NativeArray<int>(1000, Allocator.Temp);
//            NativeArray<int> wakingArray = new NativeArray<int>(1000, Allocator.Temp);


//            Entities.ForEach((Entity e, ActionGraphAsset asset, ref ActionRunState state) =>
//            {
//                var id = state.InstanceID;
//                var index = state.Index;
//                ref ActionStateContainer container = ref ActionStateMapToAsset.Instance.GetContainer(id);

//                var stateData = container.GetStateForEntity(index);// state.State;
//                if (stateData.AnyActive == false && stateData.AnyWaking == false) return;

//                var nodeList = asset.Asset.RuntimeNodes;
//                context.CurrentEntity = e;
//                context.TargetEntity = e;
//                context.StateData = stateData;
//                context.Graph = asset.Asset;
//                context.EM = EntityManager;
//                context.PostCommand = PostUpdateCommands;


//                var (activeCount, wakingCount) = stateData.GetAllActiveOrWakingIndex(ref actives, ref wakingArray);

//                for (int i = 0; i < activeCount; i++)
//                {
//                    context.Index = actives[i];
//                    var node = nodeList[actives[i]] as IStatusNode;

//                    node.OnTick(ref context);
//                }
//                for (int i = 0; i < wakingCount; i++)
//                {
//                    var wakingIndex = wakingArray[i]; 
//                    context.Index = wakingIndex;
//                    stateData.RemoveNodeCycle(wakingIndex, NodeCycle.Waking);
//#if UNITY_EDITOR
//                    if (nodeList[wakingIndex] is ISleepable == false)
//                    {
//                        throw new System.Exception($"{nodeList[wakingIndex]} 没有实现 ISleepable接口");
//                    }
//#endif

//                    ((ISleepable)nodeList[wakingIndex]).Wake(ref context);

//                    //var node = nodeList[wakingIndex] as ISleepable;
//                    //node.Wake(ref context);
//                }

//            });
//        }
//    }