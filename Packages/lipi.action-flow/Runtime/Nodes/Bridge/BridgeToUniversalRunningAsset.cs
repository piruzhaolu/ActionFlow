using System;
using UnityEngine;

namespace ActionFlow
{
    
    [Serializable]
    [NodeInfo("Bridge/ToUniversal(Status)")]
    public class BridgeToUniversalRunningAsset: INode,IBehaviorNode,INodeInput
    {
        [HideInInspector]
        public int _i; //无意义,暂时处理无数据类型序列化出错
        
        [NodeOutput]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            context.NodeOutput();
            return BehaviorStatus.Running;
        }

        public void OnInput(ref Context context)
        {
            context.BehaviorRunningCompleted(BehaviorStatus.Success);
            
        }

    }
    
    
    
    [Serializable]
    [NodeInfo("Wait")]
    public class Wait : INode,INodeInput, ISleepable 
    {
        public float Time;

        [NodeOutput]
        public void Wake(ref Context context)
        {
            context.NodeOutput();
           
        }
        public void OnInput(ref Context context)
        {
            context.SetWakeTimerAndSleep(this, Time);
        }
    }
}