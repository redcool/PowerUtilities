using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Experimental.Playables;
using UnityEngine.Playables;

namespace PowerUtilities
{
    public static class PlayableGraphTools
    {
        /// <summary>
        /// Create anim graph and output
        /// </summary>
        /// <param name="anim"></param>
        /// <param name="graph"></param>
        /// <param name="animOutput"></param>
        /// <param name="graphName"></param>
        public static void CreateGraph(Animator anim, ref PlayableGraph graph, ref AnimationPlayableOutput animOutput, string graphName = "play graph")
        {
            if (!graph.IsValid())
                graph = PlayableGraph.Create(graphName);

            if (!animOutput.IsOutputValid())
                animOutput = AnimationPlayableOutput.Create(graph, $"{graphName} anim output", anim);
        }

        public static void TryDestory(this PlayableGraph graph)
        {
            if (graph.IsValid())
                graph.Destroy();
        }

        public static AnimationPlayableOutput CreateAnimOutput(this PlayableGraph graph,Animator anim)
        {
            return AnimationPlayableOutput.Create(graph, $"{graph.GetEditorName()} anim output", anim);
        }

        public static ScriptPlayableOutput CreateScriptOutput(this PlayableGraph graph)
        {
            return ScriptPlayableOutput.Create(graph, $"{graph.GetEditorName()} script output {graph.GetOutputCount()}");
        }

        public static AudioPlayableOutput CreateAudioOutput(this PlayableGraph graph,AudioSource target)
        {
            return AudioPlayableOutput.Create(graph, $"{graph.GetEditorName()} audio output {graph.GetOutputCount()}",target);
        }

        public static TexturePlayableOutput CreateTextureOutput(this PlayableGraph graph,RenderTexture rt)
        {
            return TexturePlayableOutput.Create(graph, $"{graph.GetEditorName()} rt output {graph.GetOutputCount()}", rt);
        }

        /// <summary>
        /// create ScriptPlayable<PlayableBeheviour> within graph
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <param name="inputCount"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static ScriptPlayable<T> CreateScript<T>(this PlayableGraph graph, int inputCount, params float[] weights) where T : PlayableBehaviour, new()
        {
            var sp = ScriptPlayable<T>.Create(graph, inputCount);
            if (weights != null)
            {
                for (int i = 0; i < inputCount; i++)
                    sp.SetInputWeight(i, weights[i]);
            }

            return sp;
        }

        /// <summary>
        /// ClipPlayable is leaf node
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="clip"></param>
        /// <param name="inputCount"></param>
        /// <returns></returns>
        public static AnimationClipPlayable CreateClip(this PlayableGraph graph, AnimationClip clip)
        {
            var cp = AnimationClipPlayable.Create(graph, clip);
            // set duration when no loop otherwise duration is double.Max
            if (!clip.isLooping)
                cp.SetDuration(clip.length);

            return cp;
        }

        public static AnimationClipPlayable[] CreateClips(this PlayableGraph graph,params AnimationClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            var playables = new AnimationClipPlayable[clips.Length];
            for (int i = 0; i < clips.Length; i++)
            {
                playables[i] = CreateClip(graph, clips[i]);
            }

            return playables;
        }

        public static void CreateClips(this PlayableGraph graph,Dictionary<AnimationClip,AnimationClipPlayable> playDict, params AnimationClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return;

            for (int i = 0; i < clips.Length; i++)
            {
                playDict[clips[i]] = CreateClip(graph, clips[i]);
            }
        }

        public static AnimatorControllerPlayable CreateAnimator(this PlayableGraph graph, RuntimeAnimatorController controller)
        {
            var p = AnimatorControllerPlayable.Create(graph, controller);
            return p;
        }
        public static AnimationMixerPlayable CreateMixer(this PlayableGraph graph, params AnimationClip[] clips)
        {
            var plays = clips.Select(clip => (Playable)CreateClip(graph, clip)).ToArray();
            return CreateMixer(graph, plays);
        }

        public static AnimationMixerPlayable CreateMixer(this PlayableGraph graph, params Playable[] plays)
        {
            AnimationMixerPlayable mixer = AnimationMixerPlayable.Create(graph, plays.Length);
            for (int i = 0; i < plays.Length; i++)
            {
                var play = plays[i];
                var weight = i == 0 ? 1 : 0;
                mixer.ConnectInput(i, play, 0, weight);
            }
            return mixer;
        }

        public static AnimationLayerMixerPlayable CreateLayerMixer(this PlayableGraph graph, params Playable[] plays)
        {
            var mixer = AnimationLayerMixerPlayable.Create(graph, plays.Length);
            for (int i = 0;i < plays.Length; i++)
            {
                var play = plays[i];
                var weight = i == 0 ? 1 : 0;
                mixer.ConnectInput(i, play, 0, weight);
            }
            return mixer;
        }

        public static AnimationLayerMixerPlayable CreateLayerMixer(this PlayableGraph graph, params (Playable play, float weight,AvatarMask avatarMask,bool isAdditiveLayer)[] layerInfos)
        {
            AnimationLayerMixerPlayable mixer = AnimationLayerMixerPlayable.Create(graph, layerInfos.Length);
            for (int i = 0; i < layerInfos.Length; i++)
            {
                var item = layerInfos[i];
                
                mixer.ConnectInput(i, item.play, 0,item.weight);
                if (item.avatarMask)
                    mixer.SetLayerMaskFromAvatarMask((uint)i, item.avatarMask);

                mixer.SetLayerAdditive((uint)i, item.isAdditiveLayer);
            }
            return mixer;
        }

    }
}