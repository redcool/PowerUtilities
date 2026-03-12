using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_SPLINES
using UnityEngine.Splines;
#endif
using UnityEngine;

namespace PowerUtilities
{

    [Serializable]
    public class LockTargetInfo
    {
        public enum TargetOffetMode
        {
            Add=0,Multiply
        }
        public bool isEnable;
        public Transform target;
        public Vector3 offset;
        public TargetOffetMode offsetMode;
        [Range(0,1)] public float smoothTime = 0.2f;

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
    public class LockRotationInfo
    {
        public enum TargetRotateMode
        {
            LookAtTarget, SyncTargetRotation
        }

        public bool isEnable;
        public Transform target;
        public TargetRotateMode rotationMode;
        public float speed = 1f; // angular speed in radians per second
        public void SetupRotationVec(ref Vector3 vec,Transform tr)
        {
            switch (rotationMode)
            {
                case TargetRotateMode.LookAtTarget: vec = (target.position - tr.position); break;
                case TargetRotateMode.SyncTargetRotation: vec = target.forward; break;
            }
        }
    }

    [Serializable]
    public class LockSpineInfo
    {
        public enum RotationMode
        {
            LookForward = 0, LookAtTarget,
        }
        public bool isEnable;
#if UNITY_SPLINES
        [Header("spline")]
        public SplineContainer spline;
        public int splineIndex = 0;

        [Header("Play")]
        public float playTime=0.2f;// how long playTime from [0,1]
        public bool isAutoRate = true;
        public PlayMode playMode;

        [Header("Rate")]
        [Range(0,1)] public float curRate = 0f;

        [Header("pos")]
        public Vector3 offset;
        public LockTargetInfo.TargetOffetMode offsetMode;
        public float smoothTime = 0.2f;

        [Header("Rotation")]
        public bool isEnableRotation;
        public RotationMode rotationmode;
        public float rotateSpeed = 1f; //
        public Transform target;

        float endRate = 1f;

        public Vector3 GetSplinePos(out Vector3 nextPos ,out Vector3 tangent,out Vector3 up)
        {
            tangent = Vector3.forward;
            up = Vector3.up;
            nextPos = Vector3.zero;

            if (!spline)
                return Vector3.zero;

            if (isAutoRate)
                MathTools.MoveTowards(ref curRate, ref endRate, playMode, playTime);

            var nextRate = curRate + 0.01f;
            nextPos = spline.EvaluatePosition(splineIndex, nextRate);

            tangent = spline.EvaluateTangent(splineIndex, curRate);
            up = spline.EvaluateUpVector(splineIndex, curRate);

            return spline.EvaluatePosition(splineIndex, curRate);
        }

        public Vector3 GetSplinePos(out Vector3 nextPos)
        {
            nextPos = Vector3.zero;

            if (!spline)
                return Vector3.zero;

            if (isAutoRate)
                MathTools.MoveTowards(ref curRate, ref endRate, playMode, playTime);

            var nextRate = curRate + 0.01f;
            nextPos = spline.EvaluatePosition(splineIndex, nextRate);

            return spline.EvaluatePosition(splineIndex, curRate);
        }

        public bool IsValid() => isEnable && spline && playTime > 0;

        public Vector3 GetRotationDir(Vector3 curPos, Vector3 nextPos)
        {
            if (rotationmode == RotationMode.LookAtTarget && target)
                return target.position - curPos;

            return nextPos -curPos;
        }
#endif
    }

    [ExecuteAlways]
    public class TransformConstraint : MonoBehaviour
    {

        public LockTargetInfo positionLock = new ();

        public LockRotationInfo rotateLock = new ();
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
            rotateLock.SetupRotationVec(ref targetForward, transform);

            //transform.forward
            var stepDir = Vector3.RotateTowards(transform.forward, targetForward, rotateLock.speed * Time.deltaTime, 0);
            transform.rotation = Quaternion.LookRotation(stepDir);
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
            var splinePos = splineLock.GetSplinePos(out var nextPos);
            LockTargetInfo.SetupOffset(ref splinePos, splineLock.offset, splineLock.offsetMode);

            Vector3 velocity = default;
            transform.position = Vector3.SmoothDamp(transform.position, splinePos, ref velocity, splineLock.smoothTime);

            var rotationDir = splineLock.GetRotationDir(splinePos, nextPos);
            if (splineLock.isEnableRotation && rotationDir != Vector3.zero)
            {
                var stepDir = Vector3.RotateTowards(transform.forward, rotationDir, splineLock.rotateSpeed * Time.deltaTime, 0);
                transform.rotation = Quaternion.LookRotation(stepDir);
            }
#endif
        }
    }
}
