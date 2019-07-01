using UnityEngine;
using System.Collections;
using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace ActionFlow
{
    public unsafe struct NativeStaticMap : IDisposable
    {

        public static NativeStaticMap CreateMap(ref NativeStaticMapHead head, int size)
        {
            var map = new NativeStaticMap
            {
                head = head,
                data = (byte*)UnsafeUtility.Malloc(head.Size * size, 4, Allocator.Persistent)
            };
            return map;
        }

        private NativeStaticMapHead head;
        private byte* data;




        public void Dispose()
        {
            head.Dispose();
            UnsafeUtility.Free(data, Allocator.Persistent);
        }
    }
}
