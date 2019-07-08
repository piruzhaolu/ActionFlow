using UnityEngine;
using System.Collections;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{
    public struct NativeStaticMapHead:IDisposable
    {
        public struct TypePosition
        {
            public int TypeIndex;
            public int Offset;
            public int Size;
        }

        internal NativeArray<TypePosition> TypePositions; //每位的类别数据

        internal int Size; //总的内存大小

        public bool GetPosition<T>(out TypePosition pos) where T : struct
        {
            var index = TypeManager.GetTypeIndex<T>();
            return GetPosition(index, out pos);
        }

        public bool GetPosition(Type type, out TypePosition pos)
        {
            var index = TypeManager.GetTypeIndex(type);
            return GetPosition(index, out pos);
        }

        public bool GetPosition(int typeIndex, out TypePosition pos)
        {
            for (int i = 0; i < TypePositions.Length; i++)
            {
                pos = TypePositions[i];
                if (TypePositions[i].TypeIndex == typeIndex) return true;
            }
            pos = default;
            return false;
        }

        public void Dispose()
        {
            TypePositions.Dispose();
        }


        ///==========  Builder
        ///==========  Builder
        ///==========  Builder

        public static Builder CreateBuilder()
        {
            var b = new Builder();
            return b;
        }

        public class Builder
        {
            internal NativeList<TypePosition> NativeLists;
            private int offset;

            public Builder()
            {
                NativeLists = new NativeList<TypePosition>(Allocator.Temp);
                offset = 0;
            }

            public Builder Add<T>() where T : struct
            {
                var index = TypeManager.GetTypeIndex<T>();

                for (int i = 0; i < NativeLists.Length; i++)
                {
                    if (NativeLists[i].TypeIndex == index) return this;
                }

                var size = UnsafeUtility.SizeOf<T>();
                NativeLists.Add(new TypePosition()
                {
                    TypeIndex = index,
                    Offset = offset,
                    Size = size
                });
                offset += size;
                return this;
            }


            public NativeStaticMapHead ToHead()
            {
                var head = new NativeStaticMapHead()
                {
                    TypePositions = NativeLists.ToArray(Allocator.Persistent),
                    Size = offset
                };
                NativeLists.Dispose();
                return head;
            }

        }
    }

}
