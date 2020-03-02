using System;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{
    [Serializable]
    public struct NullStatus { public byte b; } // b数据无意义，增加是因为无法序列化0 size的struct


    [Serializable]
    public struct ParallelStatus
    {
        public ulong Value;

        public BehaviorStatus this[int i]
        {
            set
            {
                var v = (ulong)value & 0b1111;
                v <<= (i * 4);
                Value |= v;
            }
            get
            {
                var v = Value >> (i * 4);
                return (BehaviorStatus)(v & 0b1111);
            }
        }
    }

    /// <summary>
    /// 带状态node的抽象类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StatusNodeBase<T> : IStatusNode<T> where T:struct
    {
        public abstract void OnTick(ref Context context);

        public unsafe int CreateNodeDataTo(byte* b)
        {
            var v = new T();
            var size = UnsafeUtility.SizeOf<T>();
            if (size != 0)
            {
                UnsafeUtility.CopyStructureToPtr(ref v, b);
            }
            return size;
        }

        public Type NodeDataType()
        {
            return typeof(T);
        }
        
    }
}
