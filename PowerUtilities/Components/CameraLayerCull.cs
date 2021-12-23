namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CameraLayerCull : MonoBehaviour
    {
        public Camera cam;
        public float[] cullingDistances;
        public bool isSphereCulling = false;
        // Start is called before the first frame update
        void OnEnable()
        {
            if (!cam)
                cam = Camera.main;
            if (!cam)
                return;

            SetCameraLayerCull(cam, isSphereCulling, cullingDistances);
        }

        public static void SetCameraLayerCull(Camera cam, bool isSphereCulling, float[] cullingDistances)
        {
            if (!cam)
                return;

            cam.layerCullSpherical = isSphereCulling;
            cam.layerCullDistances = cullingDistances;
        }

    }
}