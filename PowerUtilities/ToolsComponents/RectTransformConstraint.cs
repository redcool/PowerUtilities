using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [ExecuteAlways]
    public class RectTransformConstraint : MonoBehaviour
    {
        public LockTargetInfo positionLock = new LockTargetInfo();
        public Camera cam;

        RectTransform rt;

        // Start is called before the first frame update
        void OnEnable()
        {
            rt = GetComponent<RectTransform>();

            if (!cam)
                cam = Camera.main;
        }

        private void Update()
        {
            LockPosition();
        }

        private void LockPosition()
        {
            if (!positionLock.isLockPosition || !positionLock.target)
                return;

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;

            var targetPos = positionLock.target.position;
            LockTargetInfo.SetupOffset(ref targetPos, positionLock.offset, positionLock.offsetMode);

            var anchoredPos = cam.WorldToScreenPoint(targetPos);
            Vector3 velocity = default;
            rt.anchoredPosition = Vector3.SmoothDamp(rt.anchoredPosition3D, anchoredPos, ref velocity, positionLock.smoothTime);
        }

    }
}
