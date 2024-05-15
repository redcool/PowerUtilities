namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using Debug = UnityEngine.Debug;

    /// <summary>
    /// planar reflection camera 
    /// </summary>
    [ExecuteAlways]
    public class PlanarReflectionCameraControl : MonoBehaviour
    {
        [Header("Reflection Camera")]
        
        [Tooltip("property name in shader")]
        public string reflectionTextureName = "_ReflectionTexture";

        [Tooltip("which layer reflection camera can see")]
        public LayerMask layers = -1;

        [Tooltip("reflection camera clear options")]
        public CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
        public Color backgroundColor;

        [Tooltip("reflection camera's tag,urp pass use this control pass")]
        public string cameraTag = "ReflectionCamera";

        [Tooltip("texture's scale")]
        [Range(0,4)]public int downSamples = 1;
        [Tooltip("auto generate mipmaps")]
        public bool isGenerateMips = true;

        [Header("Plane ")]
        [Tooltip("water plane's height, when reflectionPlaneTr is empty ")]
        public float planeYOffset;

        [Tooltip("mirror plane,result mirror effects")]
        public Transform reflectionPlaneTr;

        [Header("Main Camera")]
        public Camera mainCam;

        [Header("Gizmos")]
        [Min(1)]public int drawLineBoxCount = 3;

        [Header("Debug")]
        [EditorDisableGroup] public Camera reflectionCam;
        [EditorDisableGroup] public RenderTexture reflectionRT;
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
            var addData = mainCam.GetUniversalAdditionalCameraData();
            var renderScale = UniversalRenderPipeline.asset.renderScale;

            // avoid console error
            var samples = Application.isPlaying ? downSamples : 0;

            var width = mainCam.pixelWidth>> samples;
            var height = mainCam.pixelHeight >> samples;

            TryCreateReflectionRT(ref reflectionRT, width, height, isGenerateMips);
            SetupReflectionCameraStates();
            RenderReflection();
            SendToShader();
        }
        private void OnDestroy()
        {
            if (reflectionRT)
                reflectionRT.Destroy();
        }

        private void TryCreateReflectionRT(ref RenderTexture rt, int width, int height, bool isGenerateMips)
        {
            if (!rt || lastWidth != width || lastHeight != height)
            {
                lastWidth = width;
                lastHeight = height;

                var w = Mathf.Max(lastWidth, 2);
                var h = Mathf.Max(lastHeight, 2);

                rt = new RenderTexture(w, h, 16);
                rt.useMipMap = isGenerateMips;
                rt.autoGenerateMips = isGenerateMips;
                //rt.Create();
                Debug.Log("create "+ lastWidth);
            }
            if (rt)
            {
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
            SetupCameraAdditionalData(cam);
            return cam;
        }

        private static void SetupCameraAdditionalData(Camera cam)
        {
            var addData = cam.GetUniversalAdditionalCameraData();
            if (!addData)
                return;

            addData.renderShadows = false;
            addData.requiresColorOption = CameraOverrideOption.Off;
            addData.requiresDepthTexture = false;
            addData.renderPostProcessing = false;
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