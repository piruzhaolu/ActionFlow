using System;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{
    [Serializable]
    public struct NullStatus { }


    [Serializable]
    public struct ParallelStatus
    {
        public ulong Value;

        public BehaviorStatus this[int i]
        {
            set
            {
                ulong v = (ulong)value & 0b1111;
                v <<= (i * 4);
                Value |= v;
            }
            get
            {
                ulong v = Value >> (i * 4);
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


        //public T GetValue(ref Context context)
        //{
        //    return context.StateData.GetValue<T>(context.Index);
        //}


        //public void SetValue(ref Context context, T value)
        //{
        //    context.StateData.SetValue(context.Index, value);
        //}

        
    }
}
