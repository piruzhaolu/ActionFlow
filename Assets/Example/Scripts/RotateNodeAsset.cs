using UnityEngine;
using System.Collections;
using ActionFlow;
using Unity.Transforms;
using Unity.Mathematics;

public struct RotateRandom
{
    public float Value;
}

[System.Serializable]
public class RotateNode : StatusNodeBase<RotateRandom>, INodeInput
{
    [NodeInputParm]
    public float RotateValue = 0.1f;

    public void OnInput(ref Context context)
    {
        context.Active();
        SetValue(ref context, new RotateRandom()
        {
            Value = UnityEngine.Random.Range(0.1f, 0.2f)
        });
    }

    public override void OnTick(ref Context context)
    {
        var rv = context.GetParameter(RotateValue);
        var randomVal = GetValue(ref context);
        var r = context.EM.GetComponentData<Rotation>(context.CurrentEntity);
        r.Value = math.mul(r.Value , quaternion.RotateY(rv + randomVal.Value));
        context.EM.SetComponentData(context.CurrentEntity, r);
    }
}


[NodeInfo("Example/Rotate")]
public class RotateNodeAsset : NodeAsset<RotateNode>
{

}