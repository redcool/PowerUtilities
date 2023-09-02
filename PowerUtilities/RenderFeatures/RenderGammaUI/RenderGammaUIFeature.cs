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

            [Tooltip("ui objects use")]
            [LoadAsset("UI-Default1.shader")]
            public Shader uiShader;

            public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
            public int passEventOffset = 10;
            public LayerMask layerMask = 32;

            [Tooltip("Define ui camera use this tag, otherwise will check automatic(1 linear space,2 overlay camera,3 camera cullingMask is UI)")]
            public string cameraTag;

            [Header("Fullsize Texture")]
            [Tooltip("create other full size texture,when RenderingScale < 1, rendering objects in fullscreen,FSR need this text")]
            public bool createFullsizeGammaTex;

            [Tooltip("Need use stencil buffer?")]
            public bool useStencilBuffer=true;
            public StencilStateData stencilStateData;

            [Header("Performance Options")]
            [Tooltip("Best option is close for Middle device.")]
            public bool disableFSR = true;

            [Tooltip("No blit,no gamma texture,draw in linear space")]
            public bool isWriteToCameraTarget;

            [Header("Editor Options")]
            public bool reset;
            public string logs;


            Material uiMat;
            public Material UIMaterial
            {
                get
                {
                    if (!uiMat && uiShader)
                        uiMat = new Material(uiShader);
                    return uiMat;
                }
            }
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
            settings.logs = "";
            if (!settings.blitMat)
            {
                settings.logs = "settings.blitMat not exists";
                return;
            }

            ref var cameraData = ref renderingData.cameraData;
            if (!IsUICamera(ref cameraData, settings.cameraTag))
            {
                settings.logs = "UICamera not found";
                return;
            }

            if ((cameraData.camera.cullingMask & settings.layerMask) == 0)
            {
                settings.logs = "UICamera.cullingMask != settings.layerMask";
                return;
            }
            if (uiPass == null)
                uiPass  = new RenderUIPass();

            uiPass.renderPassEvent = settings.passEvent+settings.passEventOffset;
            uiPass.settings = settings;

            renderer.EnqueuePass(uiPass);
        }

    }


}