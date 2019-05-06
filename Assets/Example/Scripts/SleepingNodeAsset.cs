using UnityEngine;
using System.Collections;
using ActionFlow;
using Unity.Entities;

public struct SleepComponent:IComponentData
{
    public float t;
}


[System.Serializable]
public class SleepingNode : StatusNodeBase<NullStatus>, IBehaviorNode
{
    public BehaviorStatus BehaviorInput(ref Context context)
    {
        context.TransferToSystemAndSleep(new SleepComponent()
        {
            t = Time.time
        });
        return BehaviorStatus.Running;
    }

    public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
    {
        throw new System.NotImplementedException();
    }

    public override void OnTick(ref Context context)
    {
        Debug.Log("Tick");
        context.Inactive();
        context.BehaviorRunningCompleted(BehaviorStatus.Success);
    }
}


[NodeInfo("Example/SleepingNode")]
public class SleepingNodeAsset : NodeAsset<SleepingNode>
{

    
}
