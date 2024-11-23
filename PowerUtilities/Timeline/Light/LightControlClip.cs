using System;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class LightControlClip : PlayableAsset
    {
        //---1 
        //public ExposedReference<Light> lightRef;
        //---2 
        //public Color color = Color.white;
        //public float intensity = 1;
        //public AnimationCurve intensityCurve;

        public LightControlBehaviour template;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var sb = ScriptPlayable<LightControlBehaviour>.Create(graph,template);
            //var b = sb.GetBehaviour();
            //---2
            //b.color = color;
            //b.intensity = intensity;

            //---1
            //b.light2 = lightRef.Resolve(graph.GetResolver());
            return sb;
        }
    }
}
