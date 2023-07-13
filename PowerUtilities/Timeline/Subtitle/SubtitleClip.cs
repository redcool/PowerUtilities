using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class SubtitleClip : PlayableAsset
    {
        public string text;
        public Color color;

        [NotKeyable]
        public SubtitleBehaviour subTitle = new SubtitleBehaviour();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var subtitle = ScriptPlayable<SubtitleBehaviour>.Create(graph, subTitle);
            var b = subtitle.GetBehaviour() ;
            b.text = text ;
            b.color = color;
            return subtitle;
        }
    }
}
