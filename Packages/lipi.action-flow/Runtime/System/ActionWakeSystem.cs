//using UnityEngine;
//using System.Collections;
//using Unity.Entities;
//using Unity.Transforms;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;
//
//namespace ActionFlow
//{
//
//    public class NullSystem : JobComponentSystem
//    {
//        private EntityQuery m_GroupTimer;
//        protected override void OnCreate()
//        {
//            m_GroupTimer = GetEntityQuery(typeof(ActionRunState));
//        }
//
//        [BurstCompile]
//        struct BBJob : IJobForEach<ActionRunState>
//        {
//            int a;
//            public void Execute(ref ActionRunState c0)
//            {
//                a++;
//            }
//        }
//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            return new BBJob().Schedule(m_GroupTimer,inputDeps);
//        }
//    }
//
//    /// <summary>
//    /// 将睡眠的Action Node 唤醒
//    /// </summary>
//    [DisableAutoCreation]
//    public class ActionWakeSystem : JobComponentSystem
//    {
//        private EntityQuery m_Group;
//        private EntityQuery m_GroupTimer;
//        protected override void OnCreate()
//        {
//            m_Group = GetEntityQuery(typeof(NodeSleeping));
//            m_GroupTimer = GetEntityQuery(typeof(NodeTimer), typeof(ActionRunState));
//        }
//
//        struct RemoveItem
//        {
//            public Entity Entity;
//            public int NodeIndex;
//        }
//
//        struct WakeItem
//        {
//            public int InstanceID;
//            public int ChunkIndex;
//            public int NodeIndex;
//        }
//       
//
//
//        [BurstCompile]
//        struct WakeJob : IJobChunk
//        {
//            public ArchetypeChunkBufferType<NodeSleeping> NodeSleepings;
//            public NativeQueue<RemoveItem>.ParallelWriter RemoveList;
//
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                var sleepings = chunk.GetBufferAccessor(NodeSleepings);
//                var chunkComponents = chunk.Archetype.GetComponentTypes(Allocator.Temp);
//
//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    var buffers = sleepings[i];
//                    for (int j = buffers.Length - 1; j >= 0; j--)
//                    {
//                        var sleepingItem = buffers[j];
//                        if (chunkComponents.IndexOf(sleepingItem.ComponentType) == -1)
//                        {
//                            buffers.RemoveAt(j);
//                            RemoveList.Enqueue(new RemoveItem() { Entity = sleepingItem.Entity, NodeIndex = sleepingItem.NodeIndex }); 
//                            //ActionRunStates[sleepingItem.Entity].State.SetNodeCycle(sleepingItem.NodeIndex, ActionStateData.NodeCycle.Waking);
//                        }
//                    }
//                }
//            }
//        }
//
//        [BurstCompile]
//        struct WakeWithTimerJob : IJobChunk
//        {
//            public ArchetypeChunkBufferType<NodeTimer> NodeTimeBufferType;
//            public ArchetypeChunkComponentType<ActionRunState> ActionRunStateType;
//            public float dt;
//            public NativeQueue<WakeItem>.ParallelWriter RemoveList;
//
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                var nodeTimers = chunk.GetBufferAccessor(NodeTimeBufferType);
//                var states = chunk.GetNativeArray(ActionRunStateType);
//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    var buffers = nodeTimers[i];
//                    for (int j = buffers.Length-1; j >=0; j--)
//                    {
//                        var item = buffers[j];
//                        item.Time -= dt;
//                        if (item.Time < 0)
//                        {
//                            RemoveList.Enqueue(new WakeItem()
//                            {
//                                InstanceID = states[i].InstanceID,
//                                ChunkIndex = states[i].ChunkIndex,
//                                NodeIndex = item.NodeIndex
//                            });
//                            buffers.RemoveAt(j);
//                        }
//                        else
//                        {
//                            buffers[j] = item;
//                        }
//                    }
//                }
//            }
//        }
//
//
//
//
//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            var removeList = new NativeQueue<RemoveItem>(Allocator.TempJob);
//            var timerRemoveList = new NativeQueue<WakeItem>(Allocator.TempJob);
//            var job = new WakeJob()
//            {
//                NodeSleepings = GetArchetypeChunkBufferType<NodeSleeping>(),
//                RemoveList = removeList.AsParallelWriter(),
//            };
//
//            var jobTimer = new WakeWithTimerJob()
//            {
//                dt = Time.DeltaTime,
//                NodeTimeBufferType = GetArchetypeChunkBufferType<NodeTimer>(),
//                ActionRunStateType = GetArchetypeChunkComponentType<ActionRunState>(),
//                RemoveList = timerRemoveList.AsParallelWriter(),
//
//            };
//
//            var handle = job.Schedule(m_Group, inputDeps);
//            var handleTimer = jobTimer.Schedule(m_GroupTimer, inputDeps);
//
//            handle.Complete();
//            handleTimer.Complete();
//
//            if (removeList.Count > 0)
//            {
//                var actionRunStates = GetComponentDataFromEntity<ActionRunState>();
//                while (removeList.TryDequeue(out var item))
//                {
//                    var states = actionRunStates[item.Entity];
//                    var container = ActionStateMapToAsset.Instance.GetContainer(states.InstanceID);
//                    container.SetNodeCycle(new ActionStateIndex() { ChunkIndex = states.ChunkIndex, NodeIndex = item.NodeIndex }, NodeCycle.Waking);
//                }
//            }
//
//            if (timerRemoveList.Count > 0)
//            {
//                while(timerRemoveList.TryDequeue(out var item))
//                {
//                    var container = ActionStateMapToAsset.Instance.GetContainer(item.InstanceID);
//                    container.SetNodeCycle(new ActionStateIndex() { ChunkIndex = item.ChunkIndex, NodeIndex = item.NodeIndex }, NodeCycle.Waking);
//                }
//            }
//
//            removeList.Dispose();
//            timerRemoveList.Dispose();
//            return inputDeps;
//        }
//
//
//    }
//
//}
