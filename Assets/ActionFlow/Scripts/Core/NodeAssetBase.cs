using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ActionFlow
{
    public abstract class NodeAssetBase<T> : ScriptableObject, IStatusNode where T : struct
    {

        public abstract void OnTick(ref ActionFlowContext context);

        public Type NodeDataType()
        {
            return typeof(T);
        }

        public T GetValue(ref ActionFlowContext context)
        {
            return context.StateData.GetValue<T>(context.Index);
        }

        public void SetValue(ref ActionFlowContext context, T value)
        {
            context.StateData.SetValue(context.Index, value);
        }


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


    }
}