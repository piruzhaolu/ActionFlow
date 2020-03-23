using UnityEngine;
using System.Collections;
using ActionFlow;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;


[NodeInfo("Game/Rotate")]
[System.Serializable]
public class RotateNode : INode, IBehaviorNode
{
    public float RotateValueS;
    public float RotateValueE;
    
    private Random _random;
    
    public BehaviorStatus BehaviorInput(ref Context context)
    {
        if (_random.state == 0 || _random.state == 0x6E624EB7u)
        {
            _random= new Random(5584);
        }
        
        var rValue = _random.NextFloat(RotateValueS, RotateValueE);
        var r = context.EntityManager.GetComponentData<Rotation>(context.CurrentEntity);
        r.Value = math.mul(r.Value, quaternion.RotateY(rValue));
        context.EntityManager.SetComponentData(context.CurrentEntity, r);
        
        return BehaviorStatus.Success;
    }

    
}
