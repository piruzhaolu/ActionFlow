using UnityEngine;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System;

namespace ActionFlow
{

    public unsafe struct ActionStateContainer:IDisposable
    {

        private NativeArray<ActionStateNode> Nodes;
        private byte* States;
        private NativeList<ActionStateChunk> Chunks;

        private int statesSize;//每块状态大小
        private int nodeCount;//每块节点数量

        private int chunkCount;
        private int chunkCapacity;



        public static ActionStateContainer Create(GraphAsset graph, int chunkCapacity = 5)
        {
            var container = new ActionStateContainer();
            var count = graph.RuntimeNodes.Length;
            container.nodeCount = count;
            container.Chunks = new NativeList<ActionStateChunk>(Allocator.Persistent);
           // container.Chunks.Add(new ActionStateChunk());
            container.Nodes = new NativeArray<ActionStateNode>(count* chunkCapacity, Allocator.Persistent);
            container.chunkCount = 0;

            var capacity = 1000;
            var statePtr = (byte*)UnsafeUtility.Malloc(capacity, 4, Allocator.Temp);

            int offset = 0;
            for (int i = 0; i < count; i++)
            {
                var asset = graph.Nodes[i] as INodeAsset;
                var node = new ActionStateNode() { Cycle = NodeCycle.Inactive, offset = offset };
                container.Nodes[i] = node;

                if (asset?.GetValue() is IStatusNode nodeObject)
                {
                    var t = nodeObject.NodeDataType();
                    var size = UnsafeUtility.SizeOf(t);
                    if (offset + size > capacity)
                    {
                        capacity = capacity * 2;
                        var ptr = (byte*)UnsafeUtility.Malloc(capacity, 4, Allocator.Temp);
                        UnsafeUtility.MemCpy(ptr, statePtr, offset);
                        UnsafeUtility.Free(ptr, Allocator.Temp);
                        statePtr = ptr;
                    }
                    nodeObject.CreateNodeDataTo(statePtr + offset);
                    offset += size;
                }
            }
            container.States = (byte*) UnsafeUtility.Malloc(offset* chunkCapacity, 4, Allocator.Persistent);
            UnsafeUtility.MemCpy(container.States, statePtr, offset);
            UnsafeUtility.Free(statePtr, Allocator.Temp);

            container.statesSize = offset;
            container.nodeCount = count;
            container.chunkCapacity = chunkCapacity;

            return container;
        }

        public int AddChunk()
        {
            if (chunkCapacity <= chunkCount)
            {
                var newChunkCapacity = chunkCapacity * 2;
                var newNodes = new NativeArray<ActionStateNode>(newChunkCapacity * nodeCount, Allocator.Persistent);
                NativeArray<ActionStateNode>.Copy(Nodes, 0, newNodes, 0, chunkCapacity* nodeCount);
                Nodes.Dispose();
                Nodes = newNodes;


                var newStates = UnsafeUtility.Malloc(newChunkCapacity * statesSize, 4, Allocator.Persistent);
                UnsafeUtility.MemCpy(newStates, States, chunkCapacity * statesSize);
                UnsafeUtility.Free(States, Allocator.Persistent);
                States = (byte*)newStates;
                
                chunkCapacity = newChunkCapacity;
            }

            Chunks.Add(new ActionStateChunk()
            {
                Position = chunkCount
            });
            //var v = Nodes[chunkCount * nodeCount];
            //v.Cycle = NodeCycle.Active;
            //Nodes[chunkCount * nodeCount] = v;
            chunkCount++;

            return chunkCount-1;
        }


        public void RemoveChunkAt(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index >= Chunks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "index 溢出");
            }
#endif
            var remove_chunk = Chunks[index];
            var last_chunk = Chunks[chunkCount - 1];
            NativeArray<ActionStateNode>.Copy(Nodes, last_chunk.Position * nodeCount, Nodes, remove_chunk.Position * nodeCount, nodeCount);
            UnsafeUtility.MemCpy(States + last_chunk.Position * statesSize, States + remove_chunk.Position * statesSize, statesSize);
            Chunks[index] = last_chunk;
            chunkCount--;
        }

        public ActionStateForEntity GetStateForEntity(int index)
        {
            return new ActionStateForEntity()
            {
                Nodes = new NativeSlice<ActionStateNode>(Nodes, Chunks[index].Position * nodeCount, nodeCount),
                Chunk = Chunks,
                States = States + Chunks[index].Position * statesSize,
                CurrChunkIndex = index
            };
        }



        public void Dispose()
        {
            Nodes.Dispose();
            UnsafeUtility.Free(States, Allocator.Persistent);
            Chunks.Dispose() ;
        }


    }

}
