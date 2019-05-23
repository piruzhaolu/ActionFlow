using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using System;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace ActionFlow
{
    [RequireComponent(typeof(Animator))]
    public class AnimationPlayer : MonoBehaviour
    {
        private PlayableGraph _playableGraph;

        private AnimationPlayableOutput playableOutput;
        private AnimationLayerMixerPlayable layerMixerPlayable;
        private AnimationMixerPlayable mixerPlayable;

        private AnimationPlayClip[] PlayingClips;

        [Range(1,10)]
        public int AllowLayer = 1;

        void Awake()
        {
            var animator = GetComponent<Animator>();
            _playableGraph = PlayableGraph.Create();
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);

            mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, 1);

            PlayingClips = new AnimationPlayClip[AllowLayer * 2];

            if (AllowLayer > 1)
            {//如果有多层混合
                layerMixerPlayable = AnimationLayerMixerPlayable.Create(_playableGraph, 1);
                _playableGraph.Connect(mixerPlayable, 0, layerMixerPlayable, 0);
                playableOutput.SetSourcePlayable(layerMixerPlayable);
                layerMixerPlayable.SetInputWeight(0, 1f);
            }
            else
            {
                playableOutput.SetSourcePlayable(mixerPlayable);
            }
            _playableGraph.Play();
        }

        public void Tick(float deltaTime)
        {
            _playableGraph.Evaluate(deltaTime);
        }


        public void Destroy()
        {
            _playableGraph.Destroy();
        }

    }

}
