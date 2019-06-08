using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine.Assertions;

namespace ActionFlow
{
    public unsafe struct NativeStructMap:IDisposable
    {
        public struct TypePosition
        {
            public int TypeIndex;
            public int Position;
        }

        public struct Builder
        {
            internal NativeList<TypePosition> NativeLists;

            private int offset;

            public void Add<T>() where T:struct
            {
                var index = TypeManager.GetTypeIndex<T>();
                for (int i = 0; i < NativeLists.Length; i++)
                {
                    if (NativeLists[i].TypeIndex == index) return;
                }

                NativeLists.Add(new TypePosition()
                {
                    TypeIndex = index,
                    Position = offset
                });
                offset += UnsafeUtility.SizeOf<T>();
            }



            public NativeStructMap ToNativeStructMap(Allocator allocator)
            {
                var map = new NativeStructMap
                {
                    allocator = allocator,
                    data = (byte*)UnsafeUtility.Malloc(offset, 4, allocator),
                    typePositions = NativeLists.ToArray(allocator),
                    Count = NativeLists.Length,
                    Capacity = offset
                };
                NativeLists.Dispose();
                return map;
            }

            public Array ToNativeStructMapArray(int length , Allocator allocator)
            {
                var data = (byte*)UnsafeUtility.Malloc(offset * length, 4, allocator);
                var map = new NativeStructMap
                {
                    allocator = allocator,
                    data = data,
                    typePositions = NativeLists.ToArray(allocator),
                    Count = NativeLists.Length,
                    Capacity = offset
                };
                return new Array()
                {
                    data = data,
                    length = length,
                    map = map
                };

            }

        }


        public unsafe struct Array:IDisposable
        {
            internal byte* data;
            internal NativeStructMap map;

            internal int length;
            private int _count;
            public int Count
            {
                internal set
                {
                    Assert.IsTrue(value > _count);
                    if (value > length)
                    {
                        var newData = (byte*)UnsafeUtility.Malloc(map.Capacity * (length+value), 4, map.allocator);
                        UnsafeUtility.MemCpy(newData, data, length * map.Capacity);
                        UnsafeUtility.Free(data, map.allocator);
                        data = newData;
                        length += value;
                    }
                    _count = value;
                }
                get { return _count; }
            }

            public NativeStructMap this[int i]
            {
                get
                {
                    var offset = map.Capacity* i;
                    var newMap = map;
                    newMap.data = data + offset;
                    return newMap;
                }
            }

            public void Dispose()
            {
                UnsafeUtility.Free(data, map.allocator);
                map.typePositions.Dispose();
            }

            public void Add()
            {
                Count++;
            }

        }




        public static Builder CreateBuilder()
        {
            var Builder = new Builder();
            Builder.NativeLists = new NativeList<TypePosition>(Allocator.Temp);
            return Builder;
        }



        private Allocator allocator;
        private byte* data;
        private NativeArray<TypePosition> typePositions;

        public int Count { private set; get; }

        public int Capacity { private set; get; }

        public ref T GetValue<T>() where T:struct
        {
            var tIndex = TypeManager.GetTypeIndex<T>();
            for (int i = 0; i < typePositions.Length; i++)
            {
                if (typePositions[i].TypeIndex == tIndex)
                {
                    var pos = typePositions[i].Position;
                    return ref UnsafeUtilityEx.AsRef<T>(data + pos);
                }
            }
            throw new Exception("Type does not exist!");
        }

        public bool TryGetValue<T>(ref T value) where T : struct
        {
            var tIndex = TypeManager.GetTypeIndex<T>();
            for (int i = 0; i < typePositions.Length; i++)
            {
                if (typePositions[i].TypeIndex == tIndex)
                {
                    var pos = typePositions[i].Position;
                    value = UnsafeUtilityEx.AsRef<T>(data + pos);
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            UnsafeUtility.Free(data, allocator);
            typePositions.Dispose();
        }
    }




}
