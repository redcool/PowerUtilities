namespace PowerUtilities.Features
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using System;
    using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif
#if UNITY_EDITOR
    [CustomEditor(typeof(RenderGammaUIFeature))]
    public class RenderGammaUIFeatureEditor : Editor
    {
    }
#endif

#if UNITY_2021_1_OR_NEWER
    [Tooltip("Render scane in linear, Render UI in gamma space")]
#endif
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {
        [EditorSettingSO(typeof(GammaUISettingSO))]
        public GammaUISettingSO settingSO;

        RenderUIPass uiPass;

        /// <inheritdoc/>
        public override void Create()
        {
        }

        public static bool IsCameraValid(ref CameraData cameraData, string cameraTag)
        {
            return string.IsNullOrEmpty(cameraTag) ? true : cameraData.camera.CompareTag(cameraTag);
        }

        private static void SetupCameraData(ref CameraData cameraData)
        {
            cameraData.clearDepth = false; // clear depth afterwards
            cameraData.requiresDepthTexture = false;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settingSO == null)
                return;

            settingSO.logs = "";
            if (!settingSO.blitMat)
            {
                settingSO.logs = "settings.blitMat not exists";
                return;
            }

            ref var cameraData = ref renderingData.cameraData;
            var isSceneCamera = cameraData.isSceneViewCamera;
            var isCameraValid = IsCameraValid(ref cameraData, settingSO.cameraTag);

            if (!isCameraValid && !isSceneCamera)
            {
                settingSO.logs = $"{cameraData.camera} not valid";
                return;
            }

            //if (isCameraValid)
            //{
            //    SetupCameraData(ref cameraData);
            //}

            // ui rendering checks
            if (settingSO.outputTarget == OutputTarget.CameraTarget)
            {
                if ((cameraData.camera.cullingMask & settingSO.filterInfo.layers) == 0)
                {
                    settingSO.logs = "UICamera.cullingMask != settings.layerMask";
                    return;
                }
            }

            if (uiPass == null)
                uiPass = new RenderUIPass();
            uiPass.SetProfileName(name);

            uiPass.renderPassEvent = settingSO.passEvent + settingSO.passEventOffset;
            uiPass.settings = settingSO;

            renderer.EnqueuePass(uiPass);
        }

    }


}