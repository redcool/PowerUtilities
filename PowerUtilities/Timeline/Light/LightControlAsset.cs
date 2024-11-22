using System;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class LightControlAsset : PlayableAsset
    {
        public ExposedReference<Light> lightRef;
        public Color color = Color.white;
        public float intensity = 1;
        public AnimationCurve intensityCurve;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sb = ScriptPlayable<LightControlBehaviour>.Create(graph);
            var b = sb.GetBehaviour();
            b.color = color;
            b.intensity = intensity;

            b.light2 = lightRef.Resolve(graph.GetResolver());
            return sb;
        }
    }
}
