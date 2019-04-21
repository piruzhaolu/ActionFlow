using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{
    public unsafe struct ActionFlowStateData
    {
        private byte* _ptr;

        public static ActionFlowStateData Create(ActionFlowGraphAsset graph)
        {

            var _allocator = Allocator.Persistent;
            var data = new ActionFlowStateData();
            var capacity = 1024;

            var v = (byte*)UnsafeUtility.Malloc(capacity, 4, _allocator);
            data._ptr = v;

            var count = graph.NodeAssets.Count;
            data.WriteLength(count);
            //data.SetRunStatus(0);
            int offset = NodeSizeOf * count + intSizeOf; //状态数据的初始偏移值


            for (int i = 0; i < count; i++)
            {
                var asset = graph.NodeAssets[i] as IStatusNode;

                var node = new Node()
                {
                    Active = 0,
                    offset = offset
                };
                UnsafeUtility.CopyStructureToPtr(ref node, data._ptr + i * NodeSizeOf + intSizeOf);
                if (asset != null)
                {
                    var t = asset.NodeDataType();
                    var size = UnsafeUtility.SizeOf(t);
                    if (offset + size > capacity)
                    {
                        capacity = capacity * 2;
                        var ptr = (byte*)UnsafeUtility.Malloc(capacity, 4, _allocator);
                        UnsafeUtility.MemCpy(ptr, data._ptr, offset);
                        UnsafeUtility.Free(data._ptr, _allocator);
                        data._ptr = ptr;
                    }
                    asset.CreateNodeDataTo(data._ptr + offset);
                    offset += size;
                }

            }



            return data;
        }

        private static int intSizeOf = UnsafeUtility.SizeOf<int>();
        private static int NodeSizeOf = UnsafeUtility.SizeOf<Node>();

        public int Length
        {
            get
            {
                UnsafeUtility.CopyPtrToStructure<int>(_ptr, out var len);
                return len;
            }
        }


        private void WriteLength(int length)
        {
            UnsafeUtility.CopyStructureToPtr(ref length, _ptr);
        }


        public T GetValue<T>(int index) where T : struct
        {
            var offset = index * NodeSizeOf + intSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            var valueOffset = node.offset;
            UnsafeUtility.CopyPtrToStructure<T>(_ptr + valueOffset, out var value);
            return value;
        }

        public void SetValue<T>(int index, T value) where T : struct
        {
            var offset = index * NodeSizeOf + intSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            var valueOffset = node.offset;
            UnsafeUtility.CopyStructureToPtr(ref value, _ptr + valueOffset);
        }

        public bool GetActiveOfIndex(int index)
        {
            var offset = index * NodeSizeOf + intSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            return node.Active == 1;
        }

        public void SetActiveOfIndex(int index, bool value)
        {
            var offset = index * NodeSizeOf + intSizeOf;
            UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            node.Active = value ? 1 : 0;
            UnsafeUtility.CopyStructureToPtr(ref node, _ptr + offset);
        }

        //判断是否有节点处理运行中
        public bool IsRun()
        {
            var len = Length;
            var offset = intSizeOf;
            var step = NodeSizeOf;
            for (int i = 0; i < len; i++)
            {
                UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset + step * i, out var node);
                if (node.Active == 1) return true;
            }
            return false;

            //UnsafeUtility.CopyPtrToStructure<Node>(_ptr + offset, out var node);
            //return node.Active == 1;
        }

        /// <summary>
        /// 获取所有已激活的索引
        /// </summary>
        /// <param name="nativeArray"></param>
        /// <returns></returns>
        public int GetAllActiveIndex(ref NativeArray<int> nativeArray)
        {
            var len = Length;
            var count = 0;
            for (int i = 0; i < len; i++)
            {
                if (GetActiveOfIndex(i))
                {
                    nativeArray[count] = i;
                    count++;
                }
            }
            return count;
        }

        public NativeSlice<int> GetAllActiveIndex()
        {
            var len = Length;
            NativeArray<int> actives = new NativeArray<int>(len, Allocator.Temp);
            var count = 0;
            for (int i = 0; i < len; i++)
            {
                if (GetActiveOfIndex(i))
                {
                    actives[count] = i;
                    count++;
                }
            }
            return new NativeSlice<int>(actives, 0, count);
        }






        public struct Node
        {
            public int Active;
            public int offset;
        }

    }
}
