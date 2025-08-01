﻿namespace PowerUtilities.Coroutine
{
    using UnityEngine;

    /// <summary>
    /// Wait for CanMoveNext return true
    /// </summary>
    public class WaitForDone
    {
        public virtual bool CanMoveNext { get; set; }
    }
    /// <summary>
    /// Wait for action return true
    /// </summary>
    public class WaitForActionDone : WaitForDone
    {
        public System.Func<bool> action;
        public WaitForActionDone(System.Func<bool> action)
        {
            this.action = action;
        }
        public override bool CanMoveNext
        {
            get
            {
                if (action != null)
                    return action();
                return true;
            }
            set => base.CanMoveNext = value;
        }
    }

    /// <summary>
    /// Wait for frames
    /// </summary>
    public class WaitForEndOfFrame : WaitForDone
    {
        public int frameCount = 1;

        public WaitForEndOfFrame(int frameCount = 1) => this.frameCount = frameCount;
        public override bool CanMoveNext
        {
            get
            {
                frameCount--;
                return frameCount <= 0;
            }
            set => base.CanMoveNext = value;
        }
    }
    /// <summary>
    /// Wait for seconds
    /// </summary>
    public class WaitForSeconds : WaitForDone
    {
        public float seconds;
        public WaitForSeconds(float seconds)
        {
            this.seconds = seconds;
        }
        public override bool CanMoveNext
        {
            get
            {
                seconds -= Time.deltaTime;
                return seconds <= 0;
            }
            set => base.CanMoveNext = value;
        }
    }
}