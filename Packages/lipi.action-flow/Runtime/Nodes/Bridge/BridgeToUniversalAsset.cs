using System;
using UnityEngine;

namespace ActionFlow
{
    [Serializable]
    [NodeInfo("Bridge/ToUniversal")]
    public class BridgeToUniversalAsset:INode, IBehaviorNode
    {
        [HideInInspector]
        public int _i; //无意义,暂时处理无数据类型序列化出错
        
        [NodeOutput]
        public BehaviorStatus BehaviorInput(ref Context context)
        {
            context.NodeOutput();
            return BehaviorStatus.Success;
        }

    }
}