using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Features
{
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public Material blitMat;
            public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
            public int passEventOffset = 10;
            public LayerMask layerMask = 32;

            [Tooltip("create other full size texture,when RenderingScale < 1, rendering objects in fullscreen")]
            public bool createFullsizeGammaTex;

            public StencilStateData stencilStateData;
            [Tooltip("Define ui camera use this tag, otherwise will check automatic(1 linear space,2 overlay camera,3 camera cullingMask is UI)")]
            public string cameraTag;

            [Header("Editor Options")]
            public bool reset;
        }
        public Settings settings;

        RenderUIPass uiPass;

        /// <inheritdoc/>
        public override void Create()
        {

        }

        public static bool IsUICamera(ref CameraData cameraData,string cameraTag)
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

            var isSceneCamera = cameraData.isSceneViewCamera;

            return isUICamera || isSceneCamera;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.blitMat)
                return;

            ref var cameraData = ref renderingData.cameraData;
            if (!IsUICamera(ref cameraData,settings.cameraTag))
                return;

            if ((cameraData.camera.cullingMask & settings.layerMask) == 0)
                return;

            if (uiPass == null || settings.reset)
            {
                settings.reset = false;

                var stencilState = new StencilState();
                stencilState.SetCompareFunction(settings.stencilStateData.stencilCompareFunction);
                stencilState.SetFailOperation(settings.stencilStateData.failOperation);
                stencilState.SetPassOperation(settings.stencilStateData.passOperation);
                stencilState.SetZFailOperation(settings.stencilStateData.zFailOperation);
                stencilState.enabled = settings.stencilStateData.overrideStencilState;

                uiPass = new RenderUIPass(settings.blitMat,
                    settings.layerMask, stencilState, settings.stencilStateData.stencilReference);
            }
            uiPass.renderPassEvent =settings.passEvent+settings.passEventOffset;
            uiPass.createFullsizeTex = settings.createFullsizeGammaTex;

            renderer.EnqueuePass(uiPass);
        }

    }


}