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

                var inputPlayable = (ScriptPlayable<VolumeControlBehaviour>)playable.GetInput(i);
                var volumeBehaviour = inputPlayable.GetBehaviour();

                //var rate = inputPlayable.GetNormalizedTime();

                var curWeight = volumeBehaviour.volumeWeight * inputWeight;

                var volume = volumeBehaviour.GetClipVolume();
                if (volume)
                {
                    // update weight
                    volume.weight = curWeight;
                    // update profile settings
                    volumeBehaviour.UpdateVolumeSettings();
                }
            }

            
        }

    }
}
