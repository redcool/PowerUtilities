using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities.Timeline
{
    public class LightControlMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var light = playerData as Light;
            var finalIntensity = 0f;
            var finalColor = Color.black;

            if (!light)
                return;

            var inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<LightControlBehaviour>)playable.GetInput(i);
                var input = inputPlayable.GetBehaviour();

                finalIntensity += input.intensity * inputWeight;
                finalColor += input.color * inputWeight;
            }

            light.intensity = finalIntensity;
            light.color = finalColor;
        }
    }
}