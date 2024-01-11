using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.Universal.RenderObjects;

namespace PowerUtilities.Features
{
    [Tooltip("Render scane in linear, Render UI in gamma space")]
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {

        public enum OutputTarget
        {
            /// <summary>
            /// device's CameraTarget
            /// </summary>
            CameraTarget,
            UrpColorTarget,
            None
        }

        [Serializable]
        public class Settings
        {
            [LoadAsset("defaultGammaUICopyColor.mat")]
            public Material blitMat;

            [Tooltip("ui objects use")]
            public bool isOverrideUIShader;
            [LoadAsset("UI-Default.shader")]
            public Shader overrideUIShader;

            public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
            public int passEventOffset = 10;

            [Header("Filter")]
            [Tooltip("main render object's layer")]
            public FilteringSettingsInfo filterInfo = new FilteringSettingsInfo
            {
                layers = 32,
                renderQueueRangeInfo = new RangeInfo(2501,5000)
            };

            [Tooltip("render objects use layers, one by one")]
            public List<FilteringSettingsInfo> filterInfoList = new List<FilteringSettingsInfo>();

            [Header("Find Camera by tag")]
            [Tooltip("Define ui camera use this tag, otherwise will check automatic(1 linear space,2 overlay camera,3 camera cullingMask is UI)")]
            public string cameraTag;

            [Header("Blit Options")]
            [Tooltip("blit to CameraTarget,URP CurrentActive,or no")]
            public OutputTarget outputTarget = OutputTarget.CameraTarget;

            [Header("Fullsize Texture")]
            [Tooltip("create a full size texture,as rendering objects target, otherwise use CameraColor(Depth)Attachment,FSR need this")]
            public bool createFullsizeGammaTex;

            [Tooltip("Need use stencil buffer?")]
            public DepthBufferBits depthBufferBits = DepthBufferBits._24;

            public StencilStateData stencilStateData;

            [Header("Performance Options")]
            [Tooltip("Best option is close for Middle device.")]
            public bool disableFSR = true;

            [Tooltip("No blit,no gamma texture,draw objects,output to camera target")]
            public bool isWriteToCameraTargetDirectly;


            [Header("Editor Options")]
            [Multiline]
            public string logs;


            Material uiMat;
            public Material UIMaterial
            {
                get
                {
                    if (!uiMat && overrideUIShader)
                        uiMat = new Material(overrideUIShader);
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
            settings.logs = "";
            if (!settings.blitMat)
            {
                settings.logs = "settings.blitMat not exists";
                return;
            }

            ref var cameraData = ref renderingData.cameraData;
            var isSceneCamera = cameraData.isSceneViewCamera;
            var isUICamera = IsUICamera(ref cameraData, settings.cameraTag);

            if (!isUICamera && !isSceneCamera)
            {
                settings.logs = "UICamera not found";
                return;
            }

            if (isUICamera)
            {
                SetupUICamera(ref cameraData);
            }

            // ui rendering checks
            if (settings.outputTarget == OutputTarget.CameraTarget)
            {
                if ((cameraData.camera.cullingMask & settings.filterInfo.layers) == 0)
                {
                    settings.logs = "UICamera.cullingMask != settings.layerMask";
                    return;
                }
            }

            if (uiPass == null)
                uiPass = new RenderUIPass();

            uiPass.renderPassEvent = settings.passEvent + settings.passEventOffset;
            uiPass.settings = settings;

            renderer.EnqueuePass(uiPass);
        }

    }


}