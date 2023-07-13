using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace PowerUtilities.Timeline
{
    [TrackBindingType(typeof(TextMeshProUGUI))]
    [TrackClipType(typeof(SubtitleClip))]
    public class SubtitleTrack : TrackAsset
    {
    }

    public class SubtitleBehaviour : PlayableBehaviour
    {
        public string text;
        public Color color = Color.white;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var tmp = playerData as TextMeshProUGUI;
            if(!tmp) return;

            tmp.text = text;

            var c = color;
            c.a *= info.weight;
            tmp.color = c;
        }
    }
}
