namespace PowerUtilities
{
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
        public static void MoveTowards(ref float curRate, ref float endRate, PlayMode playMode, float time)
        {
            //1 : is max rate
            var speed = 1 / time;

            if (playMode == PlayMode.Loop)
            {
                //curRate = Mathf.Repeat(curRate, 1f);
                if (curRate >= 1)
                {
                    curRate = 0;
                }
            }
            else if (playMode == PlayMode.PingPong)
            {
                endRate = curRate <= 0 ? 1 : curRate >= 1 ? 0f : endRate;
                //endRate = Mathf.PingPong(curRate, length);
            }
            else
                endRate = 1;

            curRate = Mathf.MoveTowards(curRate, endRate, speed * Time.deltaTime);
        }
    }
}