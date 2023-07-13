using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PowerUtilities.Timeline
{
    [TrackBindingType(typeof(Light))]
    [TrackClipType(typeof(LightControlClip))]
    public class LightControl : TrackAsset
    {
    }

    [Serializable]
    public class LightControlBehaviour : PlayableBehaviour
    {
        public Color color;
        public float intensity;
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var light = playerData as Light;
            if (!light) return;

            light.color = color;
            light.intensity = intensity;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            
        }
    }
}
