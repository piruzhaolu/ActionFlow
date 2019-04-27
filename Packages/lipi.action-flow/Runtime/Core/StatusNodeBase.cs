using System;
using Unity.Collections.LowLevel.Unsafe;

namespace ActionFlow
{
    /// <summary>
    /// 带状态node的抽象类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StatusNodeBase<T> : IStatusNode where T:struct
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
