using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

namespace PowerUtilities.Timeline
{
    [TrackBindingType(typeof(VolumeProfile))]
    [TrackClipType(typeof(VolumeControlClip))]
    public class VolumeControl : TrackAsset
    {
    }

    [Serializable]
    public class VolumeControlBehaviour : PlayableBehaviour
    {
        public VolumeProfile profile;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var profile = playerData as VolumeProfile;
            if (!profile) return;

            
        }

        public override void OnPlayableCreate(Playable playable)
        {
            
        }
    }
}
