using UnityEngine;
using System.Collections;
using ActionFlow;
using Unity.Transforms;
using Unity.Mathematics;

public struct RotateNodeData
{
    public float Value;
    public float StartTime;
}

[System.Serializable]
public class RotateNode : StatusNodeBase<RotateNodeData>, INodeInput, IBehaviorNode
{
    [NodeInputParm]
    public float RotateValue = 0.1f;

    public float TimeLength = 1;

    public BehaviorStatus BehaviorInput(ref Context context)
    {
        context.Active();
        SetValue(ref context, new RotateNodeData()
        {
            Value = 0,//UnityEngine.Random.Range(0.1f, 0.2f),
            StartTime = Time.time
        });
        return BehaviorStatus.Running;
    }

    public (bool, BehaviorStatus) Completed(ref Context context, int childIndex, BehaviorStatus result)
    {
        throw new System.NotImplementedException();
    }

    public void OnInput(ref Context context)
    {
        context.Active();
        SetValue(ref context, new RotateNodeData()
        {
            Value = UnityEngine.Random.Range(0.1f, 0.2f)
        });
    }

    public override void OnTick(ref Context context)
    {
        var rv = context.GetParameter(RotateValue);
        var data = GetValue(ref context);
        var r = context.EM.GetComponentData<Rotation>(context.CurrentEntity);
        r.Value = math.mul(r.Value, quaternion.RotateY(rv + data.Value));
        context.EM.SetComponentData(context.CurrentEntity, r);
        if (Time.time - data.StartTime > TimeLength)
        {
            context.BehaviorRunningCompleted( BehaviorStatus.Success);
            context.Inactive();
        }

    }

    
}


[NodeInfo("Example/Rotate")]
public class RotateNodeAsset : NodeAsset<RotateNode>
{

}