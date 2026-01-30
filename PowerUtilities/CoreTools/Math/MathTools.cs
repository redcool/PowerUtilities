namespace PowerUtilities
{
    using System;
    using UnityEngine;

    public enum PlayMode
    {
        Once,
        Loop,
        PingPong
    }

    public static class MathTools
    {
        public static float Frac(float v)
        {
            var n = Mathf.Abs(v);
            return n - Mathf.Floor(n);
        }
        /// <summary>
        /// MoveTowards with PlayMode
        /// 
        /// Demo:
        /// 
        /// float curRate = 0;
        /// float endRate = 1;
        /// float time =2 ;
        /// PlayMode playMode = PlayMode.Loop;
        /// 
        /// void Update(){
        ///     MathTools.MoveTowards(ref curRate, ref endRate, playMode, time);
        /// }
        /// </summary>
        /// <param name="curRate">current rate,[0,1]</param>
        /// <param name="endRate">end of rate,[0,1]</param>
        /// <param name="playMode">play mode</param>
        /// <param name="time">time of run start to end</param>
        /// <param name="maxRate">max of the rate,default is 1</param>
        public static void MoveTowards(ref float curRate, ref float endRate, PlayMode playMode, float time,float maxRate=1)
        {
            var speed = maxRate / time;

            if (playMode == PlayMode.Loop)
            {
                curRate %= maxRate;
            }
            else if (playMode == PlayMode.PingPong)
            {
                endRate = curRate <= 0 ? maxRate : curRate >= maxRate ? 0f : endRate;
            }
            else
                endRate = maxRate;

            curRate = Mathf.MoveTowards(curRate, endRate, speed * Time.deltaTime);
        }
        /// <summary>
        /// PingPong function
        /// demo:
        ///     value2 = MathTools.PingPong(Time.time, 1);
        /// </summary>
        /// <param name="t"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static float PingPong(float t, float length)
        {
            float value = t % (length * 2);
            return length - Math.Abs(value - length);
        }
    }
}