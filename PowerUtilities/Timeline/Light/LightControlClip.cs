using System;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class LightControlClip : PlayableAsset
    {
        public Color color = Color.white;
        public float intensity = 1;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sb = ScriptPlayable<LightControlBehaviour>.Create(graph);
            var b = sb.GetBehaviour();
            b.color = color;
            b.intensity = intensity;
            return sb;
        }
    }
}
