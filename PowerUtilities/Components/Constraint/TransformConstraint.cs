using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_SPLINES
using UnityEngine.Splines;
#endif
using UnityEngine;

namespace PowerUtilities
{
    public enum TargetOffetMode
    {
        Add=0,Multiply
    }
    [Serializable]
    public class LockTargetInfo
    {
        public bool isEnable;
        public Transform target;
        public Vector3 offset;
        public TargetOffetMode offsetMode;
        public float smoothTime = 0.2f;

        public static void SetupOffset(ref Vector3 vec, Vector3 offset, TargetOffetMode mode)
        {

            switch (mode)
            {
                case TargetOffetMode.Add: vec += offset; break;
                case TargetOffetMode.Multiply: vec = Vector3.Scale(offset, vec); break;
            }
        }
    }

    [Serializable]
    public class LockSpineInfo
    {
        public bool isEnable;
#if UNITY_SPLINES

        [Header("spline")]
        public SplineContainer spline;
        public int splineIndex = 0;
        public float time=0.2f;// how long time from [0,1]
        [Header("Rate")]
        public bool isAutoRate = true;
        public PlayMode playMode;

        [Range(0,1)] public float curRate = 0f;

        [Header("pos")]
        public Vector3 offset;
        public TargetOffetMode offsetMode;
        public float smoothTime = 0.2f;

        float endRate = 1f;

        public Vector3 GetSplinePos()
        {
            if (!spline)
                return Vector3.zero;

            if (isAutoRate)
                MathTools.MoveTowards(ref curRate, ref endRate, playMode, time);

            return spline.EvaluatePosition(splineIndex, curRate);
        }

        public bool IsValid() => isEnable && spline && time > 0;
#endif
    }

    [ExecuteAlways]
    public class TransformConstraint : MonoBehaviour
    {

        public LockTargetInfo positionLock = new ();

        public LockTargetInfo rotateLock = new ();
        public LockSpineInfo splineLock = new();

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            TryLockPosition();
            TryLockRotation();
            TryLockSpline();
        }

        private void TryLockRotation()
        {
            if (!rotateLock.isEnable || !rotateLock.target)
                return;

            var targetForward = rotateLock.target.forward;
            LockTargetInfo.SetupOffset(ref targetForward, rotateLock.offset, rotateLock.offsetMode);

            transform.forward = Vector3.RotateTowards(transform.forward, targetForward, rotateLock.smoothTime * Time.deltaTime, 0);
        }

        private void TryLockPosition()
        {
            if (!positionLock.isEnable || !positionLock.target)
                return;

            var targetPos = positionLock.target.position;
            LockTargetInfo.SetupOffset(ref targetPos, positionLock.offset, positionLock.offsetMode);

            Vector3 velocity = default;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, positionLock.smoothTime);
        }
        void TryLockSpline()
        {
#if UNITY_SPLINES
            if (!splineLock.IsValid())
                return;
            var spline = splineLock.spline;
            var splinePos = splineLock.GetSplinePos();
            LockTargetInfo.SetupOffset(ref splinePos, splineLock.offset, splineLock.offsetMode);

            Vector3 velocity = default;
            transform.position = Vector3.SmoothDamp(transform.position, splinePos, ref velocity, splineLock.smoothTime);
#endif
        }
    }
}
