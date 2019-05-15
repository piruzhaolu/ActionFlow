//using Unity.Burst;
//using Unity.Collections;
//using Unity.Collections.LowLevel.Unsafe;

//namespace ActionFlow
//{
//    public unsafe struct ActionStateForEntity
//    {
//        internal NativeSlice<ActionStateNode> Nodes;
//        internal byte* States;
//        internal NativeList<ActionStateChunk> Chunk;
//        internal int CurrChunkIndex;

//        public T GetValue<T>(int index) where T : struct
//        {
//            var node = Nodes[index];
//            var valueOffset = node.offset;
//            UnsafeUtility.CopyPtrToStructure<T>(States + valueOffset, out var value);
//            return value;
//        }


//        public void SetValue<T>(int index, T value) where T : struct
//        {
//            var node = Nodes[index];
//            var valueOffset = node.offset;
//            UnsafeUtility.CopyStructureToPtr(ref value, States + valueOffset);
//        }


//        public void SetNodeCycle(int index, NodeCycle cycle)
//        {
//            var node = Nodes[index];
//            var Chunk = this.Chunk[CurrChunkIndex];
//            var currCycle = node.Cycle;

//            switch (cycle)
//            {
//                case NodeCycle.Active:
//                    if (!currCycle.Has(NodeCycle.Active))
//                    {
//                        currCycle = currCycle.Add(cycle);
//                        Chunk.Active += 1;
//                    }
//                    break;
//                case NodeCycle.Sleeping:
//                    if (!currCycle.Has(NodeCycle.Sleeping))
//                    {
//                        currCycle = currCycle.Add(cycle);
//                        Chunk.Sleeping += 1;
//                        if (currCycle.Has(NodeCycle.Waking))
//                        {
//                            currCycle = currCycle.Remove(NodeCycle.Waking);
//                            Chunk.Waking -= 1;
//                        }
//                    }
//                    break;
//                case NodeCycle.Waking:
//                    if (!currCycle.Has(NodeCycle.Waking))
//                    {
//                        currCycle = currCycle.Add(cycle);
//                        Chunk.Waking += 1;
//                        if (currCycle.Has(NodeCycle.Sleeping))
//                        {
//                            currCycle = currCycle.Remove(NodeCycle.Sleeping);
//                            Chunk.Sleeping -= 1;
//                        }
//                    }
//                    break;
//                case NodeCycle.Inactive:
//                    if (currCycle.Has(NodeCycle.Active)) Chunk.Active -= 1;
//                    if (currCycle.Has(NodeCycle.Waking)) Chunk.Waking -= 1;
//                    if (currCycle.Has(NodeCycle.Sleeping)) Chunk.Sleeping -= 1;
//                    currCycle = cycle;
//                    break;
//            }
//            node.Cycle = currCycle;
//            Nodes[index] = node;
//            this.Chunk[CurrChunkIndex] = Chunk;
//        }


//        public void RemoveNodeCycle(int index, NodeCycle cycle)
//        {
//            var node = Nodes[index];
//            var Chunk = this.Chunk[CurrChunkIndex];
//            var currCycle = node.Cycle;
//            if (currCycle.Has(cycle))
//            {
//                currCycle = currCycle.Remove(cycle);
//                switch (cycle)
//                {
//                    case NodeCycle.Active: Chunk.Active -= 1; break;
//                    case NodeCycle.Sleeping: Chunk.Sleeping -= 1; break;
//                    case NodeCycle.Waking: Chunk.Waking -= 1; break;
//                }
//            }

//            node.Cycle = currCycle;
//            Nodes[index] = node;
//            this.Chunk[CurrChunkIndex] = Chunk;
//        }

//        public NodeCycle GetNodeCycle(int index)
//        {
//            return Nodes[index].Cycle;
//        }

//        public (int, int) GetAllActiveOrWakingIndex(
//            ref NativeArray<int> activeArray,
//            ref NativeArray<int> wakingArray)
//        {
//            var count_a = 0;
//            var count_b = 0;
//            for (int i = 0; i < Nodes.Length; i++)
//            {
//                var val = GetNodeCycle(i);
//                if (val.Has(NodeCycle.Active))
//                {
//                    activeArray[count_a] = i;
//                    count_a++;
//                }
//                if (val.Has(NodeCycle.Waking))
//                {
//                    wakingArray[count_b] = i;
//                    count_b++;
//                }
//            }
//            return (count_a, count_b);
//        }


//        public bool AnySleeping { get => Chunk[CurrChunkIndex].Sleeping > 0; }

//        public bool AnyActive { get => Chunk[CurrChunkIndex].Active > 0; }

//        public bool AnyWaking { get => Chunk[CurrChunkIndex].Waking > 0; }


//    }
//}
