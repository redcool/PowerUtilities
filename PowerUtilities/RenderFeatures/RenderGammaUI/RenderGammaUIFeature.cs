using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.Universal.RenderObjects;

namespace PowerUtilities.Features
{
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {
        public enum DepthBufferBits
        {
            _0 = 0, _16 = 16, _24 = 24, _32 = 32
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
            //public LayerMask layerMask = 32;

            public FilteringSettingsInfo filterInfo = new FilteringSettingsInfo
            {
                layers = 32,
                renderQueueRangeInfo = new RangeInfo(2501,5000)
            };

            [Tooltip("render objects use layers, one by one")]
            public List<FilteringSettingsInfo> filterInfoList = new List<FilteringSettingsInfo>();

            [Tooltip("Define ui camera use this tag, otherwise will check automatic(1 linear space,2 overlay camera,3 camera cullingMask is UI)")]
            public string cameraTag;

            [Header("Blit Options")]
            [Tooltip("blit to CameraTarget,check this when rendering UI")]
            public bool isFinalRendering = true;

            /**** 
             * ui rendering 
             * ***/

            [Header("Fullsize Texture")]
            [Tooltip("create other full size texture,when RenderingScale < 1, rendering objects in fullscreen,FSR need this text")]
            public bool createFullsizeGammaTex;

            [Tooltip("Need use stencil buffer?")]
            public DepthBufferBits depthBufferBits = DepthBufferBits._24;

            public StencilStateData stencilStateData;

            [Header("Performance Options")]
            [Tooltip("Best option is close for Middle device.")]
            public bool disableFSR = true;

            [Tooltip("No blit,no gamma texture,draw in linear space,output to camera target")]
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
            // ui rendering checks
            if (settings.isFinalRendering)
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