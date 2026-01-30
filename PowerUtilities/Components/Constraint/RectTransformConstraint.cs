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

        RectTransform rectTr;
        public RectTransform RectTr
        {
            get
            {
                if (!rectTr)
                    rectTr = GetComponent<RectTransform>();
                return rectTr;
            }
        }

        /// <summary>
        /// use for Hierarchy sort
        /// </summary>
        public int GetDistanceWorldCoordXZ() => positionLock.target ? Mathf.RoundToInt(positionLock.target.position.x + positionLock.target.position.z) : 0;

        public int GetDistanceToCam()
        {
            if (!cam || !positionLock.target)
                return 0;
            return Mathf.RoundToInt(Vector3.Distance(positionLock.target.position ,cam.transform.position));
        }

        // Start is called before the first frame update
        void OnEnable()
        {
            rectTr = GetComponent<RectTransform>();

            if (!cam)
                cam = Camera.main;
            if(!rectTr)
            {
                enabled = false;
            }
        }

        private void Update()
        {
            LockPosition();
        }

        private void LockPosition()
        {
            if (!positionLock.isEnable || !positionLock.target)
                return;

            rectTr.anchorMin = Vector2.zero;
            rectTr.anchorMax = Vector2.zero;

            var targetPos = positionLock.target.position;
            LockTargetInfo.SetupOffset(ref targetPos, positionLock.offset, positionLock.offsetMode);

            var anchoredPos = cam.WorldToScreenPoint(targetPos);
            Vector3 velocity = default;
            rectTr.anchoredPosition = Vector3.SmoothDamp(rectTr.anchoredPosition3D, anchoredPos, ref velocity, positionLock.smoothTime);
        }

    }
}
