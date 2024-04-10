namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using System;
    using static PowerUtilities.StringEx;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(URPBaseCamera))]
    public class URPBaseCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var inst = target as URPBaseCamera;

            if (GUILayout.Button("Set Other Overlay Cameras"))
            {
                inst.SetCameras();
                Selection.activeGameObject = inst.gameObject;
            }
        }
    }
#endif

    /// <summary>
    /// Current camera set as Base
    /// others camera set as overlay
    /// </summary>
    public class URPBaseCamera : MonoBehaviour
    {
        [Tooltip("setup overlay cameras when Start()")]
        public bool autoSetOverlays;

        [Tooltip("Camera's RenderType not Overlay will be filtered")]
        public bool isOverlayCameraOnly=true;

        [Header("Cameras Filter")]
        [Tooltip("gameObject's tag will be filtered, when not empty")]
        public string filterTag;

        [Tooltip("compare with gameObject's name, when not empty")]
        public string filterName;
        public NameMatchMode matchMode = NameMatchMode.Contains;
        // Start is called before the first frame update
        void Start()
        {
            if (autoSetOverlays)
                SetCameras();
        }

        public void SetCameras()
        {
            SetupMain(gameObject,out var mainCam, out var maincamData);

            SetupOverlays(mainCam, maincamData,isOverlayCameraOnly, IsCameraValid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cam"></param>
        /// <returns></returns>
        bool IsCameraValid(Camera cam)
        {
            if (!string.IsNullOrEmpty(filterTag))
            {
                return !cam.gameObject.CompareTag(filterTag);
            }

            if (!string.IsNullOrEmpty(filterName))
            {
                return gameObject.name.IsMatch(filterName, matchMode);
            }
            return true;
        }

        public static void SetupOverlays(Camera mainCam, UniversalAdditionalCameraData maincamData,bool isOnlyOverlayCamera=false,Func<Camera,bool> isCamValid=null)
        {
            var cams = FindObjectsOfType<Camera>();
            foreach (var cam in cams)
            {
                // check filter
                var isValid = isCamValid?.Invoke(cam);
                if (isValid.HasValue && !isValid.Value)
                    continue;

                // check main
                if (cam == mainCam)
                    continue;

                var camData = cam.gameObject.GetOrAddComponent<UniversalAdditionalCameraData>();
                if (isOnlyOverlayCamera && camData.renderType == CameraRenderType.Base)
                    continue;

                camData.renderType = CameraRenderType.Overlay;

                maincamData.cameraStack.Add(cam);
            }
        }

        public static void SetupMain(GameObject cameraGO, out Camera mainCam, out UniversalAdditionalCameraData maincamData)
        {
            mainCam = cameraGO.GetComponent<Camera>();
            maincamData = cameraGO.GetOrAddComponent<UniversalAdditionalCameraData>();

            maincamData.renderType = CameraRenderType.Base;
            maincamData.cameraStack.Clear();
        }
    }
}