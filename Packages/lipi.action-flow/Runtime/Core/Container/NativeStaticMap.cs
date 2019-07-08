using UnityEngine;
using System.Collections;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{


    public unsafe struct NativeStaticMap : IDisposable
    {
        public static NativeStaticMap Create(ref NativeStaticMapHead head, int size)
        {
            var map = new NativeStaticMap
            {
                head = head,
                Capacity = size,
                data = (byte*)UnsafeUtility.Malloc(head.Size * size, 4, Allocator.Persistent),
                _idle = new NativeQueue<int>(Allocator.Persistent)
            };
            return map;
        }

        private NativeStaticMapHead head;
        private byte* data;
        public int Count { get; private set; }
        public int Capacity { get; private set; }

        private NativeQueue<int> _idle; //闲置的队列

        public NativeStaticMapItem this[int i]
        {
            get
            {
                Debug.Assert(i < Count, "Index overflow");
                return new NativeStaticMapItem()
                {
                    data = data + head.Size * i,
                    head = head
                };
            }
        }
        public int NewItem()
        {
            if (_idle.Count > 0)
            {
                var index = _idle.Dequeue();
                UnsafeUtility.MemClear(data + head.Size * index, head.Size);
                return index;
            }

            Count += 1;
            if (Capacity < Count)
            {
                var newData = (byte*)UnsafeUtility.Malloc(head.Size * (Count + Capacity), 4, Allocator.Persistent);
                UnsafeUtility.MemCpy(newData, data, Capacity * head.Size);
                Capacity = Count + Capacity;
                UnsafeUtility.Free(data, Allocator.Persistent);
                data = newData;
            }
            return Count - 1;
        }

        public void RemoveItem(int index)
        {
            Debug.Assert(index < Count, "Index overflow");
            _idle.Enqueue(index);
        }



        public void Dispose()
        {
            head.Dispose();
            _idle.Dispose();
            UnsafeUtility.Free(data, Allocator.Persistent);
        }
    }
    public unsafe struct NativeStaticMapItem
    {
        internal byte* data;
        internal NativeStaticMapHead head;

        public T Get<T>() where T : struct
        {
            if(head.GetPosition<T>(out var pos))
            {
                UnsafeUtility.CopyPtrToStructure<T>(data + pos.Offset, out var v);
                return v;
            }
            throw new Exception("T does not exist");
        }

        public void Set<T>(T value) where T : struct
        {
            if (head.GetPosition<T>(out var pos))
            {
                UnsafeUtility.CopyStructureToPtr(ref value, data + pos.Offset);
                return;
            }
            throw new Exception("T does not exist");

        }
    }



}
