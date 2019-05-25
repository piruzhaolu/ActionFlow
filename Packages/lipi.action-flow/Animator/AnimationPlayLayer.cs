using UnityEngine;
using System.Collections;
using UnityEngine.Playables;
using System;

namespace ActionFlow
{
    [Serializable]
    public class AnimationPlayLayer
    {
        public bool IsAdditive = false;
        public AvatarMask Mask;

        [NonSerialized]
        public float Transition;
        [NonSerialized]
        public float Time;
        [NonSerialized]
        public int aInputIndex = -1;
        [NonSerialized]
        public int bInputIndex = -1;

        public void Add(int inputIndex, float transition)
        {
            Time = 0;
            Transition = transition;

            if (aInputIndex == -1)
            {
                aInputIndex = inputIndex;
                bInputIndex = -1;
            } else
            {
                bInputIndex = inputIndex;
            }
        }

        public bool GetWeight(out float aWeight, out float bWeight)
        {
            if (bInputIndex == -1 || aInputIndex == -1)
            {
                aWeight = 1;
                bWeight = 0;
                return false;
            } else if(Transition == 0)
            {
                aWeight = 0;
                bWeight = 1;
                return true;
            } else
            {
                var w = Math.Min( Time / Transition,1f);
                aWeight = 1 - w;
                bWeight = w;
                return true;
            }
        }

        public void EndBInput()
        {
            aInputIndex = bInputIndex;
            bInputIndex = -1;


        }


    }
}
