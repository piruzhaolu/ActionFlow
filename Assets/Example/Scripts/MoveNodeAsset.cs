    
using System.Runtime.CompilerServices;
using ActionFlow;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public struct MoveNodeStatus
{
    public double Start;
    public float Duration;
}


[NodeInfo("Game/Move")]
[System.Serializable]
public class MoveNodeAsset:StatusNodeBase<MoveNodeStatus>, IBehaviorNode
{
    public float DurationRangeS;
    public float DurationRangeE;
    public float Speed = 1f;
    
    private Random _random;
    public BehaviorStatus BehaviorInput(ref Context context)
    {
        if (_random.state == 0 ||  _random.state == 0x6E624EB7u)
        {
            _random = new Random(2251);
        }
        context.Active(this);
        var duration = _random.NextFloat(DurationRangeS, DurationRangeE);

        var t = (float) context.EntityManager.World.Time.ElapsedTime;
        Debug.Log($"X:{context.CurrentEntity.Index} == {t}=={duration}");
        context.SetValue(this, new MoveNodeStatus
        {
            Start = t,
            Duration = duration
        });
        return BehaviorStatus.Running;
    }

    public override void OnTick(ref Context context)
    {
        var data = context.GetValue(this);

        if (Time.time - data.Start > data.Duration)
        {
            Debug.Log($"Y:{context.CurrentEntity.Index} == {data.Start} == {data.Duration}");
            context.Inactive(this);
            context.BehaviorRunningCompleted(BehaviorStatus.Success);
        }
        else
        {
            var r = context.EntityManager.GetComponentData<Rotation>(context.CurrentEntity);
            var pos = context.EntityManager.GetComponentData<Translation>(context.CurrentEntity);
            pos.Value += math.mul(r.Value,new float3(0,0,1))*Time.deltaTime*Speed;
            context.EntityManager.SetComponentData<Translation>(context.CurrentEntity, pos);
        }
    }
}
