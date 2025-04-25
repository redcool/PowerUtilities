using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class TransformTools
    {
        /// <summary>
        /// xz plane
        /// </summary>
        /// <param name="planeY"></param>
        /// <param name="camForward"></param>
        /// <param name="camUp"></param>
        /// <param name="camPos"></param>
        public static void GetReflection(this Transform camTr, float planeY, out Vector3 camForward, out Vector3 camUp, out Vector3 camPos)
        {
            camForward = camTr.forward;
            camUp = camTr.up;
            camPos = camTr.position;
            camForward.y *= -1;
            camUp.y *= -1;
            camPos.y *= -1;

            camPos.y += planeY;
        }

        /// <summary>
        /// any plane
        /// </summary>
        /// <param name="reflectionPlane"></param>
        /// <param name="camForward"></param>
        /// <param name="camUp"></param>
        /// <param name="camPos"></param>
        public static void GetReflection(this Transform camTr, Transform reflectionPlane, out Vector3 camForward, out Vector3 camUp, out Vector3 camPos)
        {
            camForward = camTr.forward;
            camUp = camTr.up;
            camPos = camTr.position;

            var camForwardPlaneSpace = reflectionPlane.InverseTransformDirection(camForward);
            var camUpPlaneSpace = reflectionPlane.InverseTransformDirection(camUp);
            var camPosPlaneSpace = reflectionPlane.InverseTransformPoint(camPos);

            camForwardPlaneSpace.y *= -1;
            camUpPlaneSpace.y *= -1;
            camPosPlaneSpace.y *= -1;

            camForward = reflectionPlane.TransformDirection(camForwardPlaneSpace);
            camUp = reflectionPlane.TransformDirection(camUpPlaneSpace);
            camPos = reflectionPlane.TransformPoint(camPosPlaneSpace);
        }
        /// <summary>
        /// Get reflection camera transform(use reflectionPlaneTr if eists otherwise use planeYOffset)
        /// </summary>
        /// <param name="mainCamTr"></param>
        /// <param name="reflectionPlaneTr"></param>
        /// <param name="planeYOffset"></param>
        /// <param name="camForward"></param>
        /// <param name="camUp"></param>
        /// <param name="camPos"></param>
        public static void GetReflection(this Transform mainCamTr, Transform reflectionPlaneTr, float planeYOffset, out Vector3 camForward, out Vector3 camUp, out Vector3 camPos)
        {
            if (reflectionPlaneTr)
            {
                mainCamTr.GetReflection(reflectionPlaneTr, out camForward, out camUp, out camPos);
            }
            else
            {
                mainCamTr.GetReflection(planeYOffset, out camForward, out camUp, out camPos);
            }
        }

        public static void SetPosAndLookAt(this Transform camTr, Vector3 pos, Vector3 forward, Vector3 up)
        {
            camTr.position = pos;
            camTr.LookAt(pos + forward, up);
        }
        /// <summary>
        /// Setup reflection camera transform
        /// </summary>
        /// <param name="mainCamTr"></param>
        /// <param name="reflectionCamTr"></param>
        /// <param name="reflectionPlaneTr"></param>
        /// <param name="planeYOffset"></param>
        public static void SetupReflectionCameraTransform(this Transform mainCamTr,Transform reflectionCamTr,Transform reflectionPlaneTr,float planeYOffset=0)
        {
            mainCamTr.GetReflection(reflectionPlaneTr, planeYOffset, out var camForward,out var camUp, out var camPos);
            reflectionCamTr.SetPosAndLookAt(camPos, camForward, camUp);
        }
    }
}
