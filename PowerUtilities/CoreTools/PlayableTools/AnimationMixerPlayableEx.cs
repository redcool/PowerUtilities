using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PowerUtilities
{
    public static class AnimationMixerPlayableEx
    {
        /// <summary>
        /// update mixer's all input by speed in speedThresholds
        /// 
        ///     public float[] maxSpeeds = new[] { .2f, 5, 7 };
        ///     [Min(0)] public float speed;
        /// </summary>
        /// <param name="mixer"></param>
        /// <param name="speedThresholds"></param>
        /// <param name="speed"></param>
        public static void UpdateAllInputWeight(this AnimationMixerPlayable mixer, float[] speedThresholds,float speed)
        {
            //get weight and portId
            var weight = PlayableEx.CalcWeight(speedThresholds, speed, out var portId);

            // pause all input and set weight to 0
            for (int i = 0; i < mixer.GetInputCount(); i++)
            {
                mixer.SetInputWeight(i, 0);
                mixer.GetInput(i).Pause();
            }
            // update last port
            if (portId != 0)
            {
                mixer.GetInput(portId - 1).Play();
                mixer.SetInputWeight(portId - 1, 1 - weight);
            }
            // update current port
            mixer.GetInput(portId).Play();
            mixer.SetInputWeight(portId, weight);
        }
    }
}
