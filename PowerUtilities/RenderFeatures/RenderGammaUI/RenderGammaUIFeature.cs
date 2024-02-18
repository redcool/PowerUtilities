namespace PowerUtilities.Features
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using System;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Compilation;
    using System.Collections.Generic;
#endif
#if UNITY_EDITOR
    [CustomEditor(typeof(RenderGammaUIFeature))]
    public class RenderGammaUIFeatureEditor : SettingSOEditor
    {
        public override string SettingSOFieldName => base.SettingSOFieldName;
        public override Type SettingSOType => typeof(GammaUISettingSO);
    }
#endif

#if UNITY_2021_1_OR_NEWER
    [Tooltip("Render scane in linear, Render UI in gamma space")]
#endif
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {
        public GammaUISettingSO settingSO;

        RenderUIPass uiPass;

        /// <inheritdoc/>
        public override void Create()
        {
        }

        public static bool IsUICamera(ref CameraData cameraData, string cameraTag)
        {
            var isUICamera = false;

            if (string.IsNullOrEmpty(cameraTag))
            {
                isUICamera = QualitySettings.activeColorSpace == ColorSpace.Linear &&
                    cameraData.renderType == CameraRenderType.Overlay &&
                    (cameraData.camera.cullingMask & LayerMask.GetMask("UI")) >= 1
                    ;
            }
            else
            {
                isUICamera = cameraData.camera.CompareTag(cameraTag);
            }

            return isUICamera;
        }

        private static void SetupUICamera(ref CameraData cameraData)
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
            var isUICamera = IsUICamera(ref cameraData, settingSO.cameraTag);

            if (!isUICamera && !isSceneCamera)
            {
                settingSO.logs = "UICamera not found";
                return;
            }

            if (isUICamera)
            {
                SetupUICamera(ref cameraData);
            }

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