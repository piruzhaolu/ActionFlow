using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System;

namespace ActionFlow
{

    internal struct EntityAndComponentType : IEquatable<EntityAndComponentType>
    {
        public ComponentType Type;
        public Entity Entity;

        public bool Equals(EntityAndComponentType other)
        {
            return Type == other.Type && Entity == other.Entity;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() * 10000 + Entity.GetHashCode();
        }
    }

    internal struct SleepNodeState
    {
        public Entity NodeEntity;
        public int NodeIndex;
        public int State;
    }

    internal struct SleepingNode
    {
        public Entity Entity;
        public int NodeIndex;
    }

    [DisableAutoCreation]
    /// <summary>
    /// 将睡眠的Action Node 唤醒
    /// </summary>
    public class ActionWakeSystemV2 : JobComponentSystem
    {
        private NativeHashMap<ComponentType, bool> _sleepTypes;
        private NativeHashMap<EntityAndComponentType, SleepNodeState> _sleepNodes;

        //TODO: 需要做下隔离处理，当jobs对_sleepNodes进行操作时 NativeHashMap add会有冲突
        public static void AddSleepingNode(
            Entity listenEntity, ComponentType listenComponentType,
            Entity nodeEntity, int nodeIndex)
        {
            var system = World.Active.GetOrCreateSystem<ActionWakeSystemV2>();
            system._sleepTypes.TryAdd(listenComponentType, true);

            system._sleepNodes.TryAdd(new EntityAndComponentType()
            {
                Entity = listenEntity,
                Type = listenComponentType
            }, new SleepNodeState() {
                NodeIndex = nodeIndex,
                State = 0,
                NodeEntity = nodeEntity
            });

        }

        private EntityQuery m_Group;
        protected override void OnCreate()
        {
            m_Group = GetEntityQuery(typeof(NodeSleepingTag));
            _sleepTypes = new NativeHashMap<ComponentType, bool>(10, Allocator.Persistent);
            _sleepNodes = new NativeHashMap<EntityAndComponentType, SleepNodeState>(10, Allocator.Persistent);
        }


        [BurstCompile]
        struct WakeJob :IJob
        {
            [ReadOnly] public NativeHashMap<ComponentType, bool> sleepTypes;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;
            [ReadOnly] public NativeArray<ArchetypeChunk> ArchetypeChunks;

            public NativeHashMap<EntityAndComponentType, SleepNodeState> sleepNodes;


            public void Execute() //ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex
            {
                for (int ci = 0; ci < ArchetypeChunks.Length; ci++)
                {
                    var chunk = ArchetypeChunks[ci];
                    var chunkComponents = chunk.Archetype.GetComponentTypes(Allocator.Temp);
                    for (int i = 0; i < chunkComponents.Length; i++)
                    {
                        if (sleepTypes.TryGetValue(chunkComponents[i], out _))
                        {
                            if (sleepNodes.Length == 0) continue;
                            var entitys = chunk.GetNativeArray(EntityType);
                            for (int j = 0; j < entitys.Length; j++)
                            {
                                var cType = new EntityAndComponentType()
                                {
                                    Entity = entitys[j],
                                    Type = chunkComponents[i]
                                };
                                if (sleepNodes.TryGetValue(cType, out var item))
                                {
                                    item.State = 2;
                                    sleepNodes.Remove(cType);
                                    sleepNodes.TryAdd(cType, item);
                                }
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        struct WakeJob_B : IJob
        {
            [ReadOnly] public NativeHashMap<ComponentType, bool> sleepTypes;
            [ReadOnly] public ArchetypeChunkEntityType EntityType;

            public NativeList<SleepingNode> WakeNodes;
            public NativeHashMap<EntityAndComponentType, SleepNodeState> sleepNodes;
            public void Execute()
            {
                var keys = sleepNodes.GetKeyArray(Allocator.Temp);
                for (int i = 0; i < keys.Length; i++)
                {
                    if (sleepNodes.TryGetValue(keys[i],out var item)){
                        if (item.State == 0)
                        {
                            WakeNodes.Add(new SleepingNode()
                            {
                                Entity = keys[i].Entity,
                                NodeIndex = item.NodeIndex
                            });
                            sleepNodes.Remove(keys[i]);
                        }
                        else
                        {
                            item.State = 0;
                            sleepNodes.Remove(keys[i]);
                            sleepNodes.TryAdd(keys[i], item);
                        }
                    }
                }
            }
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var chunks = m_Group.CreateArchetypeChunkArray(Allocator.TempJob);
            NativeList<SleepingNode> WakeNodes = new NativeList<SleepingNode>(Allocator.TempJob);
            var job = new WakeJob()
            {
                sleepTypes = _sleepTypes,
                EntityType = GetArchetypeChunkEntityType(),
                ArchetypeChunks = chunks,
                sleepNodes = _sleepNodes,
            };
            
            var job_B = new WakeJob_B()
            {
                sleepTypes = _sleepTypes,
                sleepNodes = _sleepNodes,
                EntityType = GetArchetypeChunkEntityType(),
                WakeNodes = WakeNodes

            };

            var handle = job.Schedule(inputDeps);
            var handle_B = job_B.Schedule(handle);
            handle_B.Complete();

            for (int i = 0; i < WakeNodes.Length; i++)
            {
                var nodeInfo = WakeNodes[i];
                var runState = EntityManager.GetComponentData<ActionRunState>(nodeInfo.Entity);
                var Index = new ActionStateIndex()
                {
                    ChunkIndex = runState.ChunkIndex,
                    NodeIndex = nodeInfo.NodeIndex
                };
                ActionStateMapToAsset.Instance.GetContainer(runState.InstanceID).SetNodeCycle(Index, NodeCycle.Waking);

            }

            chunks.Dispose();
            WakeNodes.Dispose();
            return inputDeps;
        }

        protected override void OnDestroy()
        {
            _sleepTypes.Dispose();
            _sleepNodes.Dispose();
        }
    }
}



//var sleepings = chunk.GetBufferAccessor(NodeSleepings);
//var chunkComponents = chunk.Archetype.GetComponentTypes(Allocator.Temp);

//for (int i = 0; i < chunk.Count; i++)
//{
//    var buffers = sleepings[i];
//    for (int j = buffers.Length - 1; j >= 0; j--)
//    {
//        var sleepingItem = buffers[j];
//        var isDel = true;
//        for (int k = 0; k < chunkComponents.Length; k++)
//        {
//            if (chunkComponents[k] == sleepingItem.ComponentType)
//            {
//                isDel = false;
//                break;
//            }
//        }
//        if (isDel)
//        {
//            buffers.RemoveAt(j);
//            ActionRunStates[sleepingItem.Entity].State.SetNodeCycle(sleepingItem.NodeIndex, NodeCycle.Waking);
//        }

//    }
//}


/// <summary>
/// 唤醒定时器时间到了的node
/// </summary>
//public class ActionWakeWithTimerSystem : JobComponentSystem
//{
//    [RequireComponentTag(typeof(NodeTimer))]
//    struct WakeJob : IJobForEachWithEntity<ActionRunState>
//    {
//        [ReadOnly] public BufferFromEntity<NodeTimer> NodeTimers;
//        public float dt;


//        public void Execute(Entity entity, int index, ref ActionRunState c0)
//        {
//            DynamicBuffer<NodeTimer> nodeTimers = NodeTimers[entity];
//            for (int i = nodeTimers.Length - 1; i >= 0; i--)
//            {
//                var v = nodeTimers[i];
//                v.Time -= dt;
//                if (v.Time < 0)
//                {
//                    c0.State.SetNodeCycle(v.NodeIndex, NodeCycle.Waking);
//                    nodeTimers.RemoveAt(i);
//                }
//                else
//                {
//                    nodeTimers[i] = v;
//                }
//            }
//        }
//    }


//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var nodeTimers = GetBufferFromEntity<NodeTimer>(false);// GetArchetypeChunkBufferType<NodeSleeping>();


//        var job = new WakeJob()
//        {
//            NodeTimers = nodeTimers,
//            dt = Time.deltaTime
//        };
//        return job.Schedule(this, inputDeps);
//    }
//}

//public struct A : IComponentData { public int Value; }
//public struct B : IComponentData { public Entity OtherEntity; }

//public class TestSys : JobComponentSystem
//{

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        return new Job()
//        {
//            a = GetComponentDataFromEntity<A>() // read/write
//        }.Schedule(this, inputDeps);
//    }

//    [BurstCompile]
//    private struct Job : IJobProcessComponentData<B>
//    {
//        // beware of race conditions when ParallelForRestriction  disabled
//        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<A> a;
//        public void Execute([ReadOnly]ref B b)
//        {
//            Entity e = b.OtherEntity;
//            if (a.Exists(e))
//            {
//                var v = a[e].Value + 1;
//                a[e] = new A() { Value = v };
//            }
//        }
//    }

//}


//public struct A : IComponentData { public int Value; }
//public struct BBuffer : IBufferElementData
//{
//    public int Value;
//    public static implicit operator int(BBuffer item) { return item.Value; }
//    public static implicit operator BBuffer(int i) { return new BBuffer { Value = i }; }
//}

//public class TestSys : JobComponentSystem
//{

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        return new Job()
//        {
//            BBuffers = GetBufferFromEntity<BBuffer>(false) // read/write
//        }.Schedule(this, inputDeps);
//    }

//    [BurstCompile]
//    [RequireComponentTag(typeof(BBuffer))]
//    private struct Job : IJobProcessComponentDataWithEntity<A>
//    {

//        // beware of race conditions when ParallelForRestriction disabled
//        [NativeDisableParallelForRestriction] public BufferFromEntity<BBuffer> BBuffers;

//        public void Execute(Entity e, int i, [ReadOnly] ref A a)
//        {
//            DynamicBuffer<BBuffer> buffer = BBuffers[e];
//            if (buffer.Length > 0)
//                buffer[0] = a.Value;
//        }
//    }
//}