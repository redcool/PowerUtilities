using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class VolumeControlClip : PlayableAsset
    {
        public VolumeProfile profile;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sb = ScriptPlayable<VolumeControlBehaviour>.Create(graph);
            var b = sb.GetBehaviour();
            
            return sb;
        }
    }
}
