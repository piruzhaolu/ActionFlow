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

    public void Completed(ref Context context, int index)
    {
    }


    [NodeOutputBT(5)]
    [SerializeField]
    public float[] Weights;

}

[NodeInfo("Example/BTNode")]
public class BTNodeAsset : NodeAsset<BTNode>
{

    
}
