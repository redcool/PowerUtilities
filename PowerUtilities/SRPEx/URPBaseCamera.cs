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

            EditorGUITools.BeginVerticalBox(() =>
            {
                if (GUILayout.Button("Set Other Overlay Cameras"))
                {
                    inst.SetCameras(true);
                    Selection.activeGameObject = inst.gameObject;
                }
            });

        }
    }
#endif

    /// <summary>
    /// Current camera set as Base
    /// others camera set as overlay
    /// </summary>
    public class URPBaseCamera : MonoBehaviour
    {
        public enum FilterMode
        {
            Exclude,Include,
        }

        [HelpBox]
        public string helpBox = "Set current camera's renderType to Base,other cameras set to overlay, add to current camera's stack";

        [Tooltip("setup overlay cameras when Start()")]
        public bool autoSetOverlays;

        [Tooltip("Camera's RenderType not Overlay will be filtered out")]
        public bool isOverlayCameraOnly = false;
        [Tooltip("check overley cameras interval time,0 : disable")]
        [Min(0)]public float updateInterval = 0;

        // ====================== camera filter
        [EditorGroup("Camera Filter",true)]
        [Tooltip("when Tag or Name match,include camera or not")]
        public FilterMode filterMode = FilterMode.Exclude;

        [Tooltip("match gameObject tag, when not empty")]
        [EditorGroup("Camera Filter")]
        public string filterTag;

        [EditorGroup("Camera Filter")]
        [Tooltip("match gameObject name, when not empty")]
        public string filterName;

        [EditorGroup("Camera Filter")]
        public NameMatchMode filterNameMatchMode = NameMatchMode.Contains;

        Camera mainCam;
        UniversalAdditionalCameraData mainCamData;
        // Start is called before the first frame update
        void Start()
        {
            if (autoSetOverlays)
                SetCameras();
        }

        private void OnEnable()
        {
            if (updateInterval > 0f)
                InvokeRepeating(nameof(SetCameras), 1, updateInterval);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(SetCameras));
        }

        public void SetCameras(bool isForceUpdate)
        {
            if (!enabled && !isForceUpdate)
                return;
            
            SetupMain(gameObject, out mainCam, out mainCamData);

            SetupOverlays(mainCam, mainCamData, isOverlayCameraOnly, IsCameraValid);
            SortOverlays(mainCamData.cameraStack);
        }

        public void SetCameras() => SetCameras(false);


        /// <summary>
        /// Camera gameObject/s tag or name match, filtered camera out
        /// </summary>
        /// <param name="cam"></param>
        /// <returns></returns>
        bool IsCameraValid(Camera cam)
        {
            var isTagValid = true;
            var isNameValid = true;

            if (!string.IsNullOrEmpty(filterTag))
            {
                var isTagMatch = cam.CompareTag(filterTag);
                isTagValid = filterMode == FilterMode.Include ? isTagMatch : !isTagMatch;
            }

            if (!string.IsNullOrEmpty(filterName))
            {
                var isNameMatch = cam.name.IsMatch(filterName, filterNameMatchMode);
                isNameValid = filterMode == FilterMode.Include ? isNameMatch : !isNameMatch;
            }

            return isTagValid && isNameValid;
        }

        public static void SortOverlays(List<Camera> camList)
        {
            if (camList == null)
                return;

            camList.Sort((a, b) => Mathf.CeilToInt(a.depth - a.depth));
        }

        public static void SetupOverlays(Camera mainCam, UniversalAdditionalCameraData mainCamData,bool isOnlyOverlayCamera=false,Func<Camera,bool> cameraValidFunc=null)
        {
            var cams = Camera.allCameras;
            //var cams = FindObjectsOfType<Camera>();
            foreach (var cam in cams)
            {
                // check filter
                var isCamValid = cameraValidFunc?.Invoke(cam);
                if (isCamValid.HasValue && ! isCamValid.Value)
                    continue;

                // check main
                if (cam == mainCam)
                    continue;

                var camData = cam.gameObject.GetOrAddComponent<UniversalAdditionalCameraData>();
                if (isOnlyOverlayCamera && camData.renderType == CameraRenderType.Base)
                    continue;

                camData.renderType = CameraRenderType.Overlay;

                mainCamData.cameraStack.Add(cam);
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