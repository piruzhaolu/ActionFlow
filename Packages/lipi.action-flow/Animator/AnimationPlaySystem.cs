using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace ActionFlow
{
    public class AnimationPlaySystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((AnimationPlayer player) =>
            {
                player.Tick(Time.DeltaTime);
            });
        }
    }
}
