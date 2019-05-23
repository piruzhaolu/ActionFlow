using UnityEngine;
using System.Collections;
using Unity.Mathematics;

namespace ActionFlow
{
    public struct AnimationPlayClip 
    {
        public float2 Transition;
        public float Duration; //持续时间
        public int ToLayer;
        public bool IsAdditive;
        public AvatarMask Mask;

        public float Time;//已经过的时间

        public Object Clip;

        public bool IsNull => Clip == null;



    }
}
