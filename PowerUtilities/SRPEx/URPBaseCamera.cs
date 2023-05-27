namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(URPBaseCamera))]
    public class URPBaseCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var inst = target as URPBaseCamera;

            if (GUILayout.Button("Auto Set Overlay Cameras"))
            {
                inst.SetCameras();
                Selection.activeGameObject = null;
            }
        }
    }
#endif

    public class URPBaseCamera : MonoBehaviour
    {
        public bool autoSetOverlays;
        // Start is called before the first frame update
        void Start()
        {
            if (autoSetOverlays)
                SetCameras();
        }

        public void SetCameras()
        {
            var mainCam = GetComponent<Camera>();
            var maincamData = GetComponent<UniversalAdditionalCameraData>();
            if (!maincamData)
            {
                maincamData = mainCam.gameObject.AddComponent<UniversalAdditionalCameraData>();
            }

            maincamData.renderType = CameraRenderType.Base;
            maincamData.cameraStack.Clear();

            var cams = FindObjectsOfType<Camera>();
            foreach (var cam in cams)
            {
                if (cam == mainCam)
                    continue;

                var camData = cam.GetComponent<UniversalAdditionalCameraData>();
                if (!camData)
                {
                    camData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                }
                camData.renderType = CameraRenderType.Overlay;

                maincamData.cameraStack.Add(cam);
            }

        }
    }
}