//using UnityEngine;
//using System.Collections;
//using Unity.Entities;
//using Unity.Transforms;
//using Unity.Jobs;
//using Unity.Collections;
//using Unity.Burst;

//namespace ActionFlow
//{
//    /// <summary>
//    /// 将睡眠的Action Node 唤醒
//    /// </summary>
//    public class ActionWakeSystem : JobComponentSystem
//    {
//        private EntityQuery m_Group;
//        protected override void OnCreate()
//        {
//            m_Group = GetEntityQuery(typeof(NodeSleeping));
//        }


//        [BurstCompile]
//        struct WakeJob : IJobChunk
//        {
//            public ArchetypeChunkBufferType<NodeSleeping> NodeSleepings;
//            [ReadOnly] public ComponentDataFromEntity<ActionRunState> ActionRunStates;

//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                var sleepings = chunk.GetBufferAccessor(NodeSleepings);
//                var chunkComponents = chunk.Archetype.GetComponentTypes(Allocator.Temp);

//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    var buffers = sleepings[i];
//                    for (int j = buffers.Length-1; j >= 0; j--)
//                    {
//                        var sleepingItem = buffers[j];
//                        var isDel = true;
//                        for (int k = 0; k < chunkComponents.Length; k++)
//                        {
//                            if (chunkComponents[k] == sleepingItem.ComponentType)
//                            {
//                                isDel = false;
//                                break;
//                            }
//                        }
//                        if (isDel)
//                        {
//                            buffers.RemoveAt(j);
//                            ActionRunStates[sleepingItem.Entity].State.SetNodeCycle(sleepingItem.NodeIndex, NodeCycle.Waking);
//                        }
                        
//                    }
//                }
//            }
//        }

//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            var nodeSleepings = GetArchetypeChunkBufferType<NodeSleeping>();
//            var actionRunStates = GetComponentDataFromEntity<ActionRunState>();
//            var job = new WakeJob()
//            {
//                NodeSleepings = nodeSleepings,
//                ActionRunStates = actionRunStates,

//            };
            
//            return job.Schedule(m_Group, inputDeps);

//        }
//    }


//    /// <summary>
//    /// 唤醒定时器时间到了的node
//    /// </summary>
//    public class ActionWakeWithTimerSystem : JobComponentSystem
//    {
//        [RequireComponentTag(typeof(NodeTimer))]
//        struct WakeJob : IJobForEachWithEntity<ActionRunState>
//        {
//            [ReadOnly] public BufferFromEntity<NodeTimer> NodeTimers;
//            public float dt;


//            public void Execute(Entity entity, int index, ref ActionRunState c0)
//            {
//                DynamicBuffer<NodeTimer> nodeTimers = NodeTimers[entity];
//                for (int i = nodeTimers.Length-1; i >= 0; i--)
//                {
//                    var v = nodeTimers[i];
//                    v.Time -= dt;
//                    if (v.Time < 0)
//                    {
//                        c0.State.SetNodeCycle(v.NodeIndex, NodeCycle.Waking);
//                        nodeTimers.RemoveAt(i);
//                    } else
//                    {
//                        nodeTimers[i] = v;
//                    }
//                }
//            }
//        }


//        protected override JobHandle OnUpdate(JobHandle inputDeps)
//        {
//            var nodeTimers = GetBufferFromEntity<NodeTimer>(false);// GetArchetypeChunkBufferType<NodeSleeping>();
            

//            var job = new WakeJob()
//            {
//                NodeTimers = nodeTimers,
//                dt = Time.deltaTime
//            };
//            return job.Schedule(this,inputDeps);
//        }
//    }

//}


////public struct A : IComponentData { public int Value; }
////public struct B : IComponentData { public Entity OtherEntity; }

////public class TestSys : JobComponentSystem
////{

////    protected override JobHandle OnUpdate(JobHandle inputDeps)
////    {
////        return new Job()
////        {
////            a = GetComponentDataFromEntity<A>() // read/write
////        }.Schedule(this, inputDeps);
////    }

////    [BurstCompile]
////    private struct Job : IJobProcessComponentData<B>
////    {
////        // beware of race conditions when ParallelForRestriction  disabled
////        [NativeDisableParallelForRestriction] public ComponentDataFromEntity<A> a;
////        public void Execute([ReadOnly]ref B b)
////        {
////            Entity e = b.OtherEntity;
////            if (a.Exists(e))
////            {
////                var v = a[e].Value + 1;
////                a[e] = new A() { Value = v };
////            }
////        }
////    }

////}


////public struct A : IComponentData { public int Value; }
////public struct BBuffer : IBufferElementData
////{
////    public int Value;
////    public static implicit operator int(BBuffer item) { return item.Value; }
////    public static implicit operator BBuffer(int i) { return new BBuffer { Value = i }; }
////}

////public class TestSys : JobComponentSystem
////{

////    protected override JobHandle OnUpdate(JobHandle inputDeps)
////    {
////        return new Job()
////        {
////            BBuffers = GetBufferFromEntity<BBuffer>(false) // read/write
////        }.Schedule(this, inputDeps);
////    }

////    [BurstCompile]
////    [RequireComponentTag(typeof(BBuffer))]
////    private struct Job : IJobProcessComponentDataWithEntity<A>
////    {

////        // beware of race conditions when ParallelForRestriction disabled
////        [NativeDisableParallelForRestriction] public BufferFromEntity<BBuffer> BBuffers;

////        public void Execute(Entity e, int i, [ReadOnly] ref A a)
////        {
////            DynamicBuffer<BBuffer> buffer = BBuffers[e];
////            if (buffer.Length > 0)
////                buffer[0] = a.Value;
////        }
////    }
////}