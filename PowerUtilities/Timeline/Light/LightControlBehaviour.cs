using System;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
    public class LightControlBehaviour : PlayableBehaviour
    {
        public Color color;
        public float intensity;
        public AnimationCurve intensityCurve;

        public Light light2;
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var light = playerData as Light;
            if (!light) return;

            light.color = color;
            light.intensity = intensity;

            if (light2)
            {
            light2.color = color;
            light2.intensity = intensity;
            }
        }

        public override void OnPlayableCreate(Playable playable)
        {
            
        }
    }


}
