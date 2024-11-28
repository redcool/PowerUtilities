using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

namespace PowerUtilities.Timeline
{
    /// <summary>
    /// Global Volume Control track(for PostStack)
    /// </summary>
    //[TrackBindingType(typeof(VolumeProfile))]
    [TrackClipType(typeof(VolumeControlClip))]
    public class VolumeControlTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<VolumeControlMixerBehaviour>.Create(graph, inputCount);
        }

    }
}
