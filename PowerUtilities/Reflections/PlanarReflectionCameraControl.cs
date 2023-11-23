namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml.Schema;
    using UnityEngine;
    using static UnityEditor.PlayerSettings;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// planar reflection camera 
    /// </summary>
    [ExecuteAlways]
    public class PlanarReflectionCameraControl : MonoBehaviour
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
        public Transform reflectionPlaneTr;

        [Header("Main Camera")]
        public Camera mainCam;

        [Header("Gizmos")]
        [Min(3)]public int drawLineBoxCount = 10;

        Camera reflectionCam;
        RenderTexture reflectionRT;
        int lastWidth, lastHeight;

        // Start is called before the first frame update
        void OnEnable()
        {
            reflectionCam = GetOrCreateReflectionCamera("Reflection Camera");
            if (!mainCam)
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
            if (!mainCam)
                return;

            TryCreateReflectionRT(ref reflectionRT, mainCam.pixelWidth, mainCam.pixelHeight, downSamples);
            SetupReflectionCameraStates();
            RenderReflection();
            SendToShader();
        }
        private void OnDestroy()
        {
            Destroy(reflectionRT);
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

            if (reflectionPlaneTr)
            {
                GetReflection(reflectionPlaneTr, out camForward, out camUp, out camPos);
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!mainCam || !reflectionCam)
                return;

            DebugTools.DrawAxis(mainCam.transform.position, mainCam.transform.right * 10,mainCam.transform.up * 10,mainCam.transform.forward*10);
            DebugTools.DrawAxis(reflectionCam.transform.position, reflectionCam.transform.right * 10, reflectionCam.transform.up * 10, reflectionCam.transform.forward * 10);

            // draw xz plane,normal
            if (!reflectionPlaneTr)
            {
                for (int i = 0; i < drawLineBoxCount; i++)
                    DrawXZPlane(Color.green, i * 5 + 5);
                //Debug.DrawRay(Vector3.zero, Vector3.up * 10, Color.green);
                DebugTools.DrawLineArrow(Vector3.zero, Vector3.up * 10);
            }
            else
            {
                for (int i = 0; i < drawLineBoxCount; i++)
                    DrawReflectionPlane(Color.green, i * 5 + 5);
                Debug.DrawRay(reflectionPlaneTr.position, reflectionPlaneTr.up*10, Color.green);
            }


            //----------------
            void DrawXZPlane(Color c,float len=5)
            {
                var l = Vector3.left * len + new Vector3(0, planeYOffset, 0);
                var r = Vector3.right * len + new Vector3(0, planeYOffset, 0);
                var f = Vector3.forward * len + new Vector3(0, planeYOffset, 0);
                var n = Vector3.back * len + new Vector3(0, planeYOffset, 0);

                DebugTools.DrawLineStrip(new[] { l, f, r, n }, c);
            }

            void DrawReflectionPlane(Color c,float len=5)
            {
                var pos = reflectionPlaneTr.position;
                var right = reflectionPlaneTr.right * len;
                var forward = reflectionPlaneTr.forward * len;

                DebugTools.DrawLineStrip(new[] { pos - right, pos + forward, pos + right, pos - forward },c);
            }
        }


#endif

    }
}