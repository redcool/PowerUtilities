using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
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
