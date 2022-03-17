using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities
{
    public static class PlayableTools
    {
        public static AnimationClipPlayable CreateClip(PlayableGraph graph, AnimationClip clip,int inputCount=1)
        {
            var cp = AnimationClipPlayable.Create(graph, clip);
            cp.SetInputCount(inputCount);
            return cp;
        }
        public static AnimatorControllerPlayable CreateAnimator(PlayableGraph graph, RuntimeAnimatorController controller)
        {
            var p = AnimatorControllerPlayable.Create(graph, controller);

            return p;
        }

        public static AnimationMixerPlayable CreateMixer(PlayableGraph graph,params (AnimationClip clip,float weight)[] clipInfos)
        {
            var infos = new (Playable playable, float weight)[clipInfos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                infos[i].playable = CreateClip(graph, clipInfos[i].clip);
                infos[i].weight = clipInfos[i].weight;
            }
            return CreateMixer(graph,infos);
        }

        public static AnimationMixerPlayable CreateMixer(PlayableGraph graph, params (Playable playable, float weight)[] clipInfos)
        {
            AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, clipInfos.Length);
            for (int i = 0; i < clipInfos.Length; i++)
            {
                var item = clipInfos[i];
                mixer.ConnectInput(i, item.playable, 0, item.weight);
            }
            return mixer;
        }

        public static AnimationLayerMixerPlayable CreateLayerMixer(PlayableGraph graph, params (AnimationClip clip, float weight,AvatarMask avatarMask)[] clipInfos)
        {
            AnimationLayerMixerPlayable mixer = AnimationLayerMixerPlayable.Create(graph);
            for (int i = 0; i < clipInfos.Length; i++)
            {
                var item = clipInfos[i];
                var cp = CreateClip(graph,item.clip);
                mixer.ConnectInput(i, cp, 0,item.weight);
                if (item.avatarMask)
                    mixer.SetLayerMaskFromAvatarMask((uint)i, item.avatarMask);
            }
            return mixer;
        }


    }
}