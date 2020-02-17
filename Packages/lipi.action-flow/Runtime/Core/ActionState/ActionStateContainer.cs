using UnityEngine;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System;

namespace ActionFlow
{
    public unsafe struct ActionStateContainer:IDisposable
    {
        public struct Info
        {
            public int StatesSize;//每块状态大小
            public int NodeCount;//每块节点数量
            public int ChunkCount;
            public int ChunkCapacity;
        }

        private NativeArray<ActionStateNode> _nodes;
        private byte* _states;
        private NativeList<ActionStateChunk> _chunks;
        private NativeArray<Info> _containerInfo;
       // private NativeStructMap.Array BlackboardArray;
        private NativeStaticMap _blackboard;



        public static ActionStateContainer Create(GraphAsset graph, int chunkCapacity = 5)
        {
            var container = new ActionStateContainer();
           // var builder = NativeStructMap.CreateBuilder();
            var builder = NativeStaticMapHead.CreateBuilder();
            var count = graph.RuntimeNodes.Length;
            container._containerInfo = new NativeArray<Info>(1, Allocator.Persistent);
            var info = new Info {NodeCount = count};
            container._chunks = new NativeList<ActionStateChunk>(Allocator.Persistent);
            // container.Chunks.Add(new ActionStateChunk());
            container._nodes = new NativeArray<ActionStateNode>(count * chunkCapacity, Allocator.Persistent);
            info.ChunkCount = 0;

            var capacity = 1000;
            var statePtr = (byte*)UnsafeUtility.Malloc(capacity, 4, Allocator.Temp);

            var offset = 0;
            for (var i = 0; i < count; i++)
            {
                var inode = graph.m_Nodes[i];// as INodeAsset;
                var node = new ActionStateNode() { Cycle = NodeCycle.Inactive, offset = offset };
                container._nodes[i] = node;

                if (inode is IStatusNode nodeObject)
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
                if (inode is IAccessBlackboard accessBlackboard)
                {
                    accessBlackboard.ToBuilder(builder);
                }

            }
            container._states = (byte*)UnsafeUtility.Malloc(offset * chunkCapacity, 4, Allocator.Persistent);
            UnsafeUtility.MemCpy(container._states, statePtr, offset);
            UnsafeUtility.Free(statePtr, Allocator.Temp);

            //var array = builder.ToNativeStructMapArray(chunkCapacity, Allocator.Persistent);
            var head = builder.ToHead();
            var blackboard = NativeStaticMap.Create(ref head, chunkCapacity);
            //container.BlackboardArray = array;
            container._blackboard = blackboard;


            info.StatesSize = offset;
            info.NodeCount = count;
            info.ChunkCapacity = chunkCapacity;
            container._containerInfo[0] = info;


            return container;
        }



        public int AddChunk()
        {
            var info = _containerInfo[0];
            if (_containerInfo[0].ChunkCapacity <= _containerInfo[0].ChunkCount)
            {
                var newChunkCapacity = _containerInfo[0].ChunkCapacity * 2;
                var newNodes = new NativeArray<ActionStateNode>(newChunkCapacity * _containerInfo[0].NodeCount, Allocator.Persistent);
                NativeArray<ActionStateNode>.Copy(_nodes, 0, newNodes, 0, info.ChunkCapacity * info.NodeCount);
                _nodes.Dispose();
                _nodes = newNodes;


                var newStates = UnsafeUtility.Malloc(newChunkCapacity * info.StatesSize, 4, Allocator.Persistent);
                UnsafeUtility.MemCpy(newStates, _states, info.ChunkCapacity * info.StatesSize);
                UnsafeUtility.Free(_states, Allocator.Persistent);
                _states = (byte*)newStates;

                info.ChunkCapacity = newChunkCapacity;
            }

            _chunks.Add(new ActionStateChunk()
            {
                Position = info.ChunkCount
            });
            //var v = Nodes[chunkCount * nodeCount];
            //v.Cycle = NodeCycle.Active;
            //Nodes[chunkCount * nodeCount] = v;
            info.ChunkCount++;
            //BlackboardArray.Add();
            _blackboard.NewItem();
            _containerInfo[0] = info;
            return info.ChunkCount - 1;
        }


        public void RemoveChunkAt(int index)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (index < 0 || index >= _chunks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "index 溢出");
            }
#endif
            var info = _containerInfo[0];

            var removeChunk = _chunks[index];
            var lastChunk = _chunks[info.ChunkCount - 1];
            NativeArray<ActionStateNode>.Copy(
                _nodes, 
                lastChunk.Position * info.NodeCount, 
                _nodes, 
                removeChunk.Position * info.NodeCount,
                info.NodeCount);

            UnsafeUtility.MemCpy(
                _states + lastChunk.Position * info.StatesSize, 
                _states + removeChunk.Position * info.StatesSize,
                info.StatesSize);
            _chunks[index] = lastChunk;
            info.ChunkCount--;
            _containerInfo[0] = info;
        }

        //public ActionStateForEntity GetStateForEntity(int index)
        //{
        //    return new ActionStateForEntity()
        //    {
        //        Nodes = new NativeSlice<ActionStateNode>(Nodes, Chunks[index].Position * nodeCount, nodeCount),
        //        Chunk = Chunks,
        //        States = States + Chunks[index].Position * statesSize,
        //        CurrChunkIndex = index
        //    };
        //}

        #region Node数据操作

        public T GetValue<T>(ActionStateIndex index) where T : struct
        {
            var node = _nodes[ToNodeIndex(index)];
            var valueOffset = _chunks[index.ChunkIndex].Position * _containerInfo[0].StatesSize + node.offset;
            UnsafeUtility.CopyPtrToStructure<T>(_states + valueOffset, out var value);
            return value;
        }
        public void SetValue<T>(ActionStateIndex index, T value) where T : struct
        {
            var node = _nodes[ToNodeIndex(index)];
            var valueOffset = _chunks[index.ChunkIndex].Position * _containerInfo[0].StatesSize + node.offset;
            UnsafeUtility.CopyStructureToPtr(ref value, _states + valueOffset);
        }

        public void SetNodeCycle(ActionStateIndex stateIndex, NodeCycle cycle)
        {
            var index = ToNodeIndex(stateIndex);
            var node = _nodes[index];
            var chunk = _chunks[stateIndex.ChunkIndex];
            var currCycle = node.Cycle;

            switch (cycle)
            {
                case NodeCycle.Active:
                    if (!currCycle.Has(NodeCycle.Active))
                    {
                        currCycle = currCycle.Add(cycle);
                        chunk.Active += 1;
                    }
                    break;
                case NodeCycle.Sleeping:
                    if (!currCycle.Has(NodeCycle.Sleeping))
                    {
                        currCycle = currCycle.Add(cycle);
                        chunk.Sleeping += 1;
                        if (currCycle.Has(NodeCycle.Waking))
                        {
                            currCycle = currCycle.Remove(NodeCycle.Waking);
                            chunk.Waking -= 1;
                        }
                    }
                    break;
                case NodeCycle.Waking:
                    if (!currCycle.Has(NodeCycle.Waking))
                    {
                        currCycle = currCycle.Add(cycle);
                        chunk.Waking += 1;
                        if (currCycle.Has(NodeCycle.Sleeping))
                        {
                            currCycle = currCycle.Remove(NodeCycle.Sleeping);
                            chunk.Sleeping -= 1;
                        }
                    }
                    break;
                case NodeCycle.Inactive:
                    if (currCycle.Has(NodeCycle.Active)) chunk.Active -= 1;
                    if (currCycle.Has(NodeCycle.Waking)) chunk.Waking -= 1;
                    if (currCycle.Has(NodeCycle.Sleeping)) chunk.Sleeping -= 1;
                    currCycle = cycle;
                    break;
            }
            node.Cycle = currCycle;
            _nodes[index] = node;
            _chunks[stateIndex.ChunkIndex] = chunk;
        }

        public void RemoveNodeCycle(ActionStateIndex stateIndex, NodeCycle cycle)
        {
            var index = ToNodeIndex(stateIndex);
            var node = _nodes[index];
            var chunk = _chunks[stateIndex.ChunkIndex];
            var currCycle = node.Cycle;
            if (currCycle.Has(cycle))
            {
                currCycle = currCycle.Remove(cycle);
                switch (cycle)
                {
                    case NodeCycle.Active: chunk.Active -= 1; break;
                    case NodeCycle.Sleeping: chunk.Sleeping -= 1; break;
                    case NodeCycle.Waking: chunk.Waking -= 1; break;
                }
            }

            node.Cycle = currCycle;
            _nodes[index] = node;
            _chunks[stateIndex.ChunkIndex] = chunk;
        }

        public NodeCycle GetNodeCycle(ActionStateIndex stateIndex)
        {
            return _nodes[ToNodeIndex(stateIndex)].Cycle;
        }

        public (int, int) GetAllActiveOrWakingIndex(
            int chunkIndex,
            ref NativeArray<int> activeArray,
            ref NativeArray<int> wakingArray)
        {
            var countA = 0;
            var countB = 0;
            var s = _chunks[chunkIndex].Position * _containerInfo[0].NodeCount;
            var e = s + _containerInfo[0].NodeCount;
            for (var i = s; i < e; i++)
            {
                var val = _nodes[i].Cycle;
                if (val == NodeCycle.Inactive) continue;
                if (val.Has(NodeCycle.Active))
                {
                    activeArray[countA] = i-s;
                    countA++;
                }
                if (val.Has(NodeCycle.Waking))
                {
                    wakingArray[countB] = i-s;
                    countB++;
                }
            }
            return (countA, countB);
        }


        public bool AnySleeping(int chunkIndex)
        {
            return _chunks[chunkIndex].Sleeping > 0;
        }

        public bool AnyActive(int chunkIndex)
        {
            return  _chunks[chunkIndex].Active > 0;

        }

        public bool AnyWaking(int chunkIndex)
        { return _chunks[chunkIndex].Waking > 0; }


        private int ToNodeIndex(ActionStateIndex actionStateIndex)
        {
            return _chunks[actionStateIndex.ChunkIndex].Position * _containerInfo[0].NodeCount + actionStateIndex.NodeIndex;
        }
        #endregion


        public ref T GetBlackboard<T>(ActionStateIndex stateIndex) where T:struct
        {
            return ref _blackboard[stateIndex.ChunkIndex].RefGet<T>();
           //return ref BlackboardArray[stateIndex.ChunkIndex].GetValue<T>();
        }

        public void Dispose()
        {
            _nodes.Dispose();
            UnsafeUtility.Free(_states, Allocator.Persistent);
            _chunks.Dispose();
            _containerInfo.Dispose();
            _blackboard.Dispose();
            //BlackboardArray.Dispose();
        }


    }

}
