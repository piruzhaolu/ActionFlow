using UnityEngine;
using System.Collections.Generic;
using ActionFlow;
using System;


[Serializable]
public class BTNode : INode, IBehaviorNode
{
    public BehaviorStatus BehaviorInput(ref Context context)
    {
        return BehaviorStatus.Success;
    }

    public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
    {
        throw new NotImplementedException();
    }

    //public bool Completed(ref Context context, int index)
    //{
    //    return false;
    //}


    [NodeOutputBT(5)]
    [NodeInputParm]
    [SerializeField]
    public float[] Weights;

}

[NodeInfo("Example/BTNode")]
public class BTNodeAsset : NodeAsset<BTNode>
{

    
}
