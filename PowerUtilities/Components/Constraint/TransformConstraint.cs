using System;
using System.Collections;
using System.Collections.Generic;
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
        public bool isLockPosition;
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

    [ExecuteAlways]
    public class TransformConstraint : MonoBehaviour
    {

        public LockTargetInfo positionLock = new LockTargetInfo();

        public LockTargetInfo rotateLock = new LockTargetInfo();

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            LockPosition();

            LockRotation();
        }

        private void LockRotation()
        {
            if (!rotateLock.isLockPosition || !rotateLock.target)
                return;

            var targetForward = rotateLock.target.forward;
            LockTargetInfo.SetupOffset(ref targetForward, rotateLock.offset, rotateLock.offsetMode);

            transform.forward = Vector3.RotateTowards(transform.forward, targetForward, rotateLock.smoothTime * Time.deltaTime, 0);
        }

        private void LockPosition()
        {
            if (!positionLock.isLockPosition || !positionLock.target)
                return;

            var targetPos = positionLock.target.position;
            LockTargetInfo.SetupOffset(ref targetPos, positionLock.offset, positionLock.offsetMode);

            Vector3 velocity = default;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, positionLock.smoothTime);
        }


    }
}
