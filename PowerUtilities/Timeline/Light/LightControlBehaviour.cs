using System;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class LightControlBehaviour : PlayableBehaviour
    {
        public Color color = Color.white;
        public float intensity = 1;
        //public AnimationCurve intensityCurve = AnimationCurve.Linear(0,1,1,1);

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var light = playerData as Light;
            if (!light) return;

            light.color = color;
            light.intensity = intensity;

        }

    }


}
