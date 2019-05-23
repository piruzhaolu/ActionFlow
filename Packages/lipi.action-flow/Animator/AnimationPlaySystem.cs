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
            //Entities.ForEach((AnimationPlayer player, ref Translation t) =>
            //{
            //    t.Value = t.Value + new Unity.Mathematics.float3(0, 0, 0.05f);
            //});
        }
    }
}
