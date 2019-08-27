using UnityEngine;
using System.Collections;
using ActionFlow;
using Unity.Entities;

public struct SleepComponent:IComponentData
{
    public float t;
}

[NodeInfo("Example/SleepingNode")]
[System.Serializable]
public class SleepingNode : INode, IBehaviorNode, ISleepable
{
    public BehaviorStatus BehaviorInput(ref Context context)
    {
        context.TransferToSystemAndSleep(this, new SleepComponent()
        {
            t = Time.time
        });
        return BehaviorStatus.Running;
    }

   
    public void Wake(ref Context context)
    {
        context.BehaviorRunningCompleted(BehaviorStatus.Success);
    }
}



