using System;
using UnityEngine;
using UnityEngine.Playables;

namespace PowerUtilities
{
    public static class PlayableEx
    {

        /// <summary>
        /// Get playing progress [0,1] , playable need SetDuration
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double GetProgress<T>(this T a, float totalTime) where T : struct, IPlayable
        => a.GetTime() / totalTime;

        public static void SetProgress<T>(this T a, float progress, float totalTime) where T : struct, IPlayable
        {
            var time = totalTime * progress;
            a.SetTime(time);
        }

        /// <summary>
        /// Connect sourceOutputPort to target.targetInputPort 
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="play"></param>
        /// <param name="sourceOutputPort"></param>
        /// <param name="target"></param>
        /// <param name="targetInputPort"></param>
        /// <param name="targetInputWeight"></param>
        public static void ConnectOutputPort<U, V>(this U play, int sourceOutputPort, V target, int targetInputPort) where U : struct, IPlayable where V : struct, IPlayable
        {
            play.GetGraph().Connect(play, sourceOutputPort, target, targetInputPort);
        }
        /// <summary>
        /// Connect inputPort to target.targetOutputPort
        /// </summary>
        /// <typeparam name="U"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="play"></param>
        /// <param name="inputPort"></param>
        /// <param name="target"></param>
        /// <param name="targetOutputPort"></param>
        public static void ConnectInputPort<U, V>(this U play, int inputPort, V target, int targetOutputPort) where U : struct, IPlayable where V : struct, IPlayable
        {
            play.GetGraph().Connect(target, targetOutputPort, play, inputPort);
        }

        /// <summary>
        /// Calc 1d blendweight
        /// </summary>
        /// <param name="thresholds"></param>
        /// <param name="speed"></param>
        /// <param name="inputPort"></param>
        /// <returns></returns>
        public static float CalcWeight(float[] thresholds, float speed, out int inputPort)
        {
            inputPort = Array.FindIndex(thresholds, (x) => x >= speed);
            if (inputPort == 0)
                return 1;
            // not found,stay last id
            if (inputPort == -1)
                inputPort = thresholds.Length - 1;

            var minSpeed = inputPort == 0 ? 0 : thresholds[inputPort - 1];
            var maxSpeed = thresholds[inputPort];

            var weight = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

            return weight;
        }
    }
}