using UnityEngine;
using System.Collections;
using Unity.Entities;

public class SleepComponentSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, ref SleepComponent c) =>
        {
            
            if (Time.time - c.t > 10f)
            {
                PostUpdateCommands.RemoveComponent<SleepComponent>(e);
                //EntityManager.RemoveComponent<SleepComponent>(e);
            }
        });
    }
}
