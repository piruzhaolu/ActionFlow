using System;
using Unity.Mathematics;
using UnityEngine;

namespace ActionFlow
{

    public struct AnimationClipNodeData
    {

    }

    [Serializable]
    public class AnimationClipNode:StatusNodeBase<AnimationClipNodeData>, INodeInput, ISleepable
    {
        public AnimationClip AnimationClip;

        public int ToLayer = 0;

        public Vector2 Transition = new Vector2(0,0);

        public float Duration = 1f;

        public void OnInput(ref Context context)
        {
            var player = context.EntityManager.GetComponentObject<AnimationPlayer>(context.TargetEntity);
            player.Play(AnimationClip, Transition.x);
            context.SetWakeTimerAndSleep(this, Duration - Transition.y);

        }

        public override void OnTick(ref Context context)
        {
            throw new NotImplementedException();
        }

        [NodeOutput]
        public void Wake(ref Context context)
        {
            context.NodeOutput();
        }
    }



    [NodeInfo("Animation/Clip")]
    public class AnimationClipNodeAsset : NodeAsset<AnimationClipNode>
    {

    }
}
