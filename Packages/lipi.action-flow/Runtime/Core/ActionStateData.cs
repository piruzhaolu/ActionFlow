using UnityEngine;
using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Cycle = ActionFlow.ActionStateData.NodeCycle;

namespace ActionFlow
{

    public static class NodeCycleExt
    {
        public static bool Has(this Cycle self, Cycle cycle)
        {
            return (self & cycle) == cycle;
        }
        public static Cycle Add(this Cycle self, Cycle cycle)
        {
            return self | cycle;
        }

        public static Cycle Remove(this Cycle self, Cycle cycle)
        {
            return self & ~cycle;
        }

    }

    /// <summary>
    /// 存储Action动态状态的数据类
    /// </summary>
    public unsafe struct ActionStateData
    {
        public enum NodeCycle
        {
            Inactive    = 0,
            Sleeping    = 0b001,
            Active      = 0b010,
            Waking      = 0b100
        }

        


        public struct Node
        {
            public Cycle Cycle;
            public int offset;
        }

       


        private static readonly int intSizeOf = UnsafeUtility.SizeOf<int>();
        private static readonly int NodeSizeOf = UnsafeUtility.SizeOf<Node>();


        public static ActionStateData Create(GraphAsset graph)
        {
            var _allocator = Allocator.Persistent;
            var data = new ActionStateData();
            var count = graph.Nodes.Count;
            var capacity = count * 12;

            var v = (byte*)UnsafeUtility.Malloc(capacity, 4, _allocator);
            data._ptr = v;
            data.Length = count;


            int offset = NodeSizeOf * count;

            for (int i = 0; i < count; i++)
            {
                var asset = graph.Nodes[i] as INodeAsset;
                var nodeObject = asset?.GetValue() as IStatusNode;
                var node = new Node() { Cycle = Cycle.Inactive, offset = offset };
                UnsafeUtility.CopyStructureToPtr(ref node, data._ptr + i * NodeSizeOf);

                if (nodeObject != null)
                {
                    var t = nodeObject.NodeDataType();
                    var size = UnsafeUtility.SizeOf(t);
                    if (offset + size > capacity)
                    {
                        capacity = capacity * 2;
                        var ptr = (byte*)UnsafeUtility.Malloc(capacity, 4, _allocator);
                        UnsafeUtility.MemCpy(ptr, data._ptr, offset);
                        UnsafeUtility.Free(data._ptr, _allocator);
                        data._ptr = ptr;
                    }
                    nodeObject.CreateNodeDataTo(data._ptr + offset);
                    offset += size;
                }
            }

            return data;
        }

        //=======================================

        public int Length { private set; get; }
        public int Sleeping { private set; get; }
        public int Active { private set; get; }
        public int Waking { private set; get; }

        private byte* _ptr;



        //private void WriteLength(int length)
        //{
        //    UnsafeUtility.CopyStructureToPtr(ref length, _ptr);
        //}

        
        /// <summary>
        /// 取某个索引上的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="index">数据的索引值 </param>
        /// <returns></returns>
        public T GetValue<T>(int index) where T : struct
        {
            var offset = index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            var valueOffset = node.offset;
            UnsafeUtility.CopyPtrToStructure<T>(_ptr + valueOffset, out var value);
            return value;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetValue<T>(int index, T value) where T : struct
        {
            var offset = index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            var valueOffset = node.offset;
            UnsafeUtility.CopyStructureToPtr(ref value, _ptr + valueOffset);
           
        }


        public void SetNodeCycle(int index, Cycle cycle)
        {
            var cPtr = _ptr + index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(cPtr, out var node);

            var currCycle = node.Cycle;

            switch (cycle)
            {
                case Cycle.Active:
                    if (!currCycle.Has(Cycle.Active))
                    {
                        currCycle = currCycle.Add(cycle);
                        Active += 1;
                    }
                    break;
                case Cycle.Sleeping:
                    if (!currCycle.Has(Cycle.Sleeping))
                    {
                        currCycle = currCycle.Add(cycle);
                        Sleeping += 1;
                        if (currCycle.Has(Cycle.Waking))
                        {
                            currCycle = currCycle.Remove(Cycle.Waking);
                            Waking -= 1;
                        }
                    }
                    break;
                case Cycle.Waking:
                    if (!currCycle.Has(Cycle.Waking))
                    {
                        currCycle = currCycle.Add(cycle);
                        Waking += 1;
                        if (currCycle.Has(Cycle.Sleeping))
                        {
                            currCycle = currCycle.Remove(Cycle.Sleeping);
                            Sleeping -= 1;
                        }
                    }
                    break;
                case Cycle.Inactive:
                    if (currCycle.Has(Cycle.Active)) Active -= 1;
                    if (currCycle.Has(Cycle.Waking)) Waking -= 1;
                    if (currCycle.Has(Cycle.Sleeping)) Sleeping -= 1;
                    currCycle = cycle;
                    break;
            }
            node.Cycle = currCycle;
            UnsafeUtility.CopyStructureToPtr(ref node, cPtr);
        }

        public void RemoveNodeCycle(int index, Cycle cycle)
        {
            var cPtr = _ptr + index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(cPtr, out var node);
            var currCycle = node.Cycle;
            if (currCycle.Has(cycle))
            {
                currCycle = currCycle.Remove(cycle);
                switch (cycle)
                {
                    case Cycle.Active: Active -= 1;break;
                    case Cycle.Sleeping: Sleeping -= 1;break;
                    case Cycle.Waking: Waking -= 1;break;
                }
            }
            
            node.Cycle = currCycle;
            UnsafeUtility.CopyStructureToPtr(ref node, cPtr);
        }

        public Cycle GetNodeCycle(int index)
        {
            var cPtr = _ptr + index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(cPtr, out var node);
            return node.Cycle;
        }


        public (int,int) GetAllActiveOrWakingIndex(
            ref NativeArray<int> activeArray, 
            ref NativeArray<int> wakingArray)
        {
            var count_a = 0;
            var count_b = 0;
            for (int i = 0; i < Length; i++)
            {
                var val = GetNodeCycle(i);
                if (val.Has(Cycle.Active) )
                {
                    activeArray[count_a] = i;
                    count_a++;
                }
                if (val.Has(Cycle.Waking))
                {
                    wakingArray[count_b] = i;
                    count_b++;
                }
            }
            return (count_a, count_b);
        }


        public bool AnySleeping { get => Sleeping > 0; }

        public bool AnyActive { get => Active > 0; }

        public bool AnyWaking { get => Waking > 0; }




    }

}