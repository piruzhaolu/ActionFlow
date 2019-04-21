using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ActionFlow
{
    public enum BehaviorStatus
    {
        Success = 1,
        Failure = 2,
        Running = 3
    }

    public unsafe interface INodeAsset
    {
    }


    public interface INodeInput<TData> where TData : struct
    {
        void OnInput(ref ActionFlowContext context, TData inputData);
    }

    public interface INodeInput
    {
        void OnInput(ref ActionFlowContext context);
    }


    public interface IParameterType<T>
    {
        T GetValue(ref ActionFlowContext context, int nodeIndex);
    }


    public unsafe interface IStatusNode : INodeAsset
    {
        void OnTick(ref ActionFlowContext context);
        Type NodeDataType();
        int CreateNodeDataTo(byte* b);
    }

    public interface IBehaviorNode
    {
        BehaviorStatus BehaviorInput(ref ActionFlowContext context);

        void Completed(ref ActionFlowContext context, int index);
    }


    
}


