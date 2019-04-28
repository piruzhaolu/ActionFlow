using UnityEngine;
using System.Collections;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{
    /// <summary>
    /// 存储Action动态状态的数据类
    /// </summary>
    public unsafe struct ActionStateData
    {
        public enum NodeCycle
        {
            Inactive,
            Sleeping,
            Active
        }

        public struct Node
        {
            public NodeCycle Cycle;
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
                var node = new Node() { Cycle = NodeCycle.Inactive, offset = offset };
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


        public void SetNodeCycle(int index, NodeCycle cycle)
        {
            var cPtr = _ptr + index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(cPtr, out var node);
            if (node.Cycle != cycle)
            {
                if (cycle == NodeCycle.Active)
                {
                    if (node.Cycle == NodeCycle.Inactive) Active += 1;
                    else
                    {
                        Active += 1;
                        Sleeping -= 1;
                    }
                } else if (cycle == NodeCycle.Sleeping)
                {
                    if (node.Cycle == NodeCycle.Inactive) Sleeping += 1;
                    else
                    {
                        Active -= 1;
                        Sleeping += 1;
                    }
                }
                else
                {
                    if (node.Cycle == NodeCycle.Active) Active -= 1;
                    else Sleeping -= 1;
                }

                node.Cycle = cycle;
                UnsafeUtility.CopyStructureToPtr(ref node, cPtr);
            }
        }

        public NodeCycle GetNodeCycle(int index)
        {
            var cPtr = _ptr + index * NodeSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(cPtr, out var node);
            return node.Cycle;
        }


        public int GetAllActiveOrSleepingIndex(ref NativeArray<int> nativeArray)
        {
            var count = 0;
            for (int i = 0; i < Length; i++)
            {
                if (GetNodeCycle(i) != NodeCycle.Inactive)
                {
                    nativeArray[count] = i;
                    count++;
                }
            }
            return count;
        }


        public bool AllSleeping { get => Sleeping > 0 && Active == 0; }

        public bool AllInactive { get => Sleeping == 0 && Active == 0; }


    }

}