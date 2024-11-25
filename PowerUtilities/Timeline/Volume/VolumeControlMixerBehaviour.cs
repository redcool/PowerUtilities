using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    [Serializable]
    public class VolumeControlMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            // dont use track's profile
            //var profile = playerData as VolumeProfile;
            //if (!profile) return;

            var inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var inputWeight = playable.GetInputWeight(i);
                if (Mathf.Approximately(inputWeight, 0))
                {
                    continue;
                }

                var inputPlayable = (ScriptPlayable<VolumeControlBehaviour>)playable.GetInput(i);
                var b = inputPlayable.GetBehaviour();

                var rate = inputPlayable.GetNormalizedTime();

                var curWeight = b.volumeWeight * inputWeight;

                var volume = b.volumeRef.Resolve(playable.GetGraph().GetResolver()) ?? b.clipVolume;
                if (volume)
                {
                    // update weight
                    volume.weight = curWeight;
                    // update profile settings
                    volume.profile = b.clipVolumeProfile ;
                }
            }

            
        }

    }
}
