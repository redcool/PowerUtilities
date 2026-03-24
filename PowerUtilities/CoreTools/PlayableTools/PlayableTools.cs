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

        public static AnimationClipPlayable PlayClip(Animator anim,AnimationClip clip,ref PlayableGraph graph,ref AnimationPlayableOutput output)
        {
            if (!graph.IsValid())
                graph = PlayableGraph.Create("play clip graph");

            var cp = AnimationClipPlayable.Create(graph, clip);

            // set duration when no loop otherwise duration is double.Max
            if (!clip.isLooping)
                cp.SetDuration(clip.length);

            if (!output.IsOutputValid())
                output = AnimationPlayableOutput.Create(graph, "clip output", anim);

            output.SetSourcePlayable(cp);

            graph.Play();
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

        public static AnimationLayerMixerPlayable CreateLayerMixer(PlayableGraph graph, params (Playable play, float weight,AvatarMask avatarMask)[] layerInfos)
        {
            AnimationLayerMixerPlayable mixer = AnimationLayerMixerPlayable.Create(graph, layerInfos.Length);
            for (int i = 0; i < layerInfos.Length; i++)
            {
                var item = layerInfos[i];
                
                mixer.ConnectInput(i, item.play, 0,item.weight);
                if (item.avatarMask)
                    mixer.SetLayerMaskFromAvatarMask((uint)i, item.avatarMask);
            }
            return mixer;
        }

        /// <summary>
        /// Get playing progress [0,1] , playable need SetDuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double GetProgress<T>(this T a,float totalTime) where T : struct, IPlayable
        => a.GetTime() / totalTime;

        public static void SetProgress<T>(this T a,float progress,float totalTime) where T : struct, IPlayable
        {
            var time = totalTime * progress;
            a.SetTime(time);
        }
    }
}