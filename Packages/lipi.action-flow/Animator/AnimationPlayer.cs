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

        [SerializeField]
        public AnimationPlayLayer[] AnimationPlayLayers;


        void Awake()
        {
            var animator = GetComponent<Animator>();
            _playableGraph = PlayableGraph.Create();
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);

            mixerPlayable = AnimationMixerPlayable.Create(_playableGraph, 1);


            if (AnimationPlayLayers.Length > 1)
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

            for (int i = 0; i < AnimationPlayLayers.Length; i++)
            {
                var currLayer = AnimationPlayLayers[i];
                
                if (currLayer.GetWeight(out var aW, out var bW))
                {
                    mixerPlayable.SetInputWeight(currLayer.aInputIndex, aW);
                    mixerPlayable.SetInputWeight(currLayer.bInputIndex, bW);

                    if (aW <= 0)
                    {
                        mixerPlayable.DisconnectInput(currLayer.aInputIndex);
                        currLayer.EndBInput();
                    }
                }
                currLayer.Time += deltaTime;
            }
        }


        public AnimationClipPlayable Play(AnimationClip clip, float transition = 0, int toLayer = 0)
        {
            var playable = AnimationClipPlayable.Create(_playableGraph, clip);

            int inputIndex;
            if (AnimationPlayLayers.Length > 1)
            {
                var animationMixerPlayable = (AnimationMixerPlayable)layerMixerPlayable.GetInput(toLayer);
                inputIndex = GetInputIndex(animationMixerPlayable);
                _playableGraph.Connect(playable, 0, animationMixerPlayable, inputIndex);
            }
            else
            {
                if (AnimationClipIsPlaying(mixerPlayable, clip, out var playingPlayable))
                {
                    return playingPlayable;
                }
                inputIndex = GetInputIndex(mixerPlayable);
                var layer = AnimationPlayLayers[toLayer];
                layer.Add(inputIndex, transition);
                _playableGraph.Connect(playable, 0, mixerPlayable, inputIndex);
                if (layer.bInputIndex == -1)
                {
                    mixerPlayable.SetInputWeight(layer.aInputIndex, 1f);
                }
            }
            return playable;
        }

        private bool AnimationClipIsPlaying(AnimationMixerPlayable mixerPlayable, AnimationClip clip, out AnimationClipPlayable playable )
        {
            var count = mixerPlayable.GetInputCount();
            for (int i = 0; i < count; i++)
            {
                var p = mixerPlayable.GetInput(i);
                if (p.IsNull() == false)
                {
                    if (p.IsPlayableOfType<AnimationClipPlayable>())
                    {
                        playable = (AnimationClipPlayable)p;
                        if (((AnimationClipPlayable)p).GetAnimationClip() == clip) return true;
                    }
                }
            }
            playable = (AnimationClipPlayable) Playable.Null;
            return false;
        }


        // 取得可用的或新的输入索引
        private int GetInputIndex(AnimationMixerPlayable mixerPlayable)
        {
            var count = mixerPlayable.GetInputCount();
            for (int i = 0; i < count; i++)
            {
                var p = mixerPlayable.GetInput(i);
                if (p.IsNull())
                {
                    mixerPlayable.SetInputWeight(i, 0);
                    return i;
                }
            }
            mixerPlayable.SetInputCount(count + 1);
            return count; //index
        }


        public void OnDestroy()
        {
            _playableGraph.Destroy();
        }
        

    }

}
