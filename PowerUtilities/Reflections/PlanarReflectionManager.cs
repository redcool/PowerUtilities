namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [ExecuteAlways]
    public class PlanarReflectionManager : MonoBehaviour
    {
        [Header("Reflection Camera")]
        public string reflectionTextureName = "_ReflectionTexture";
        public LayerMask layers = -1;
        public CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
        public Color backgroundColor;
        public string cameraTag = "ReflectionCamera";
        [Range(0,4)]public int downSamples = 0;

        [Header("Plane ")]
        [Tooltip("use xz plane")]
        public float planeYOffset;

        [Tooltip("use any plane")]
        public Transform reflectionPlane;

        [Header("Main Camera")]
        public Camera mainCam;
        public bool autoGetMainCam = true;

        Camera reflectionCam;
        RenderTexture reflectionRT;
        int lastWidth, lastHeight;

        // Start is called before the first frame update
        void OnEnable()
        {
            reflectionCam = GetOrCreateReflectionCamera("Reflection Camera");
            if (autoGetMainCam)
            {
                mainCam = Camera.main;
            }

            if (!mainCam)
            {
                enabled = false;
                return;
            }

            
        }

        private void LateUpdate()
        {
#if UNITY_EDITOR
            if (!reflectionCam)
            {
                OnEnable();
            }
#endif
            TryCreateReflectionRT(ref reflectionRT, mainCam.pixelWidth, mainCam.pixelHeight, downSamples);
            SetupReflectionCameraStates();
            RenderReflection();
            SendToShader();
        }

        private void TryCreateReflectionRT(ref RenderTexture rt,int width,int height,int downSamples)
        {
            if (!rt || lastWidth != width || lastHeight != height)
            {
                lastWidth = width;
                lastHeight = height;

                var w = Mathf.Max(lastWidth >> downSamples, 2);
                var h = Mathf.Max(lastHeight >> downSamples, 2);

                rt = new RenderTexture(w, h, 16);
                rt.Create();
            }
        }

        private void OnDestroy()
        {
            Destroy(reflectionRT);
        }

        private void SendToShader()
        {
            Shader.SetGlobalTexture(reflectionTextureName, reflectionRT);
        }

        private void SetupReflectionCameraStates()
        {
            reflectionCam.CopyFrom(mainCam);
            reflectionCam.targetTexture = reflectionRT;
            reflectionCam.cullingMask = layers;
            reflectionCam.backgroundColor = backgroundColor;
            reflectionCam.clearFlags = clearFlags;
            reflectionCam.enabled = false;
        }

        private void RenderReflection()
        {
            Vector3 camForward, camUp, camPos;

            if (reflectionPlane)
            {
                GetReflection(reflectionPlane, out camForward, out camUp, out camPos);
            }
            else
            {
                GetReflection(planeYOffset, out camForward, out camUp, out camPos);
            }

            reflectionCam.transform.position = camPos;
            reflectionCam.transform.LookAt(camPos + camForward, camUp);

            reflectionCam.Render();
        }

        /// <summary>
        /// xz plane
        /// </summary>
        /// <param name="planeY"></param>
        /// <param name="camForward"></param>
        /// <param name="camUp"></param>
        /// <param name="camPos"></param>
        private void GetReflection(float planeY, out Vector3 camForward, out Vector3 camUp, out Vector3 camPos)
        {
            camForward = mainCam.transform.forward;
            camUp = mainCam.transform.up;
            camPos = mainCam.transform.position;
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
        void GetReflection(Transform reflectionPlane,out Vector3 camForward, out Vector3 camUp, out Vector3 camPos)
        {
            camForward = mainCam.transform.forward;
            camUp = mainCam.transform.up;
            camPos = mainCam.transform.position;

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

        Camera GetOrCreateReflectionCamera(string cameraName)
        {
            var tr = transform.Find(cameraName);
            if (!tr)
            {
                var camGo = new GameObject(cameraName);
                tr = camGo.transform;
            }
            tr.parent = transform;
            tr.gameObject.tag = cameraTag;
            var cam = tr.gameObject.GetOrAddComponent<Camera>();
            return cam;
        }

        private void OnDrawGizmos()
        {
            if (!mainCam || !reflectionCam)
                return;

            GizmosDrawArrow(mainCam.transform.position, mainCam.transform.forward * 10, Color.white);
            GizmosDrawArrow(mainCam.transform.position, mainCam.transform.up * 10, Color.green);
            GizmosDrawArrow(mainCam.transform.position, mainCam.transform.right * 10, Color.red);

            GizmosDrawArrow(reflectionCam.transform.position, reflectionCam.transform.forward * 10, Color.blue);
            GizmosDrawArrow(reflectionCam.transform.position, reflectionCam.transform.up * 10, Color.green);
            GizmosDrawArrow(reflectionCam.transform.position, reflectionCam.transform.right * 10, Color.red);

            var len = 5;
            var l = Vector3.left * len + new Vector3(0, planeYOffset, 0);
            var r = Vector3.right * len + new Vector3(0, planeYOffset, 0);
            var f = Vector3.forward * len + new Vector3(0, planeYOffset, 0);
            var n = Vector3.back * len + new Vector3(0, planeYOffset, 0);

            DebugTools.DrawLineStrip(new[] { l, f, r, n });
        }

        public static void GizmosDrawArrow(Vector3 start, Vector3 dir, Color color)
        {
            Debug.DrawLine(start, start + dir, color);
            Gizmos.DrawWireSphere(start, 0.2f);
        }
    }
}