using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Features
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
    public class GammaUISettingSO : ScriptableObject
    {
        [Header("Blit options")]
        [LoadAsset("defaultGammaUICopyColor.mat")]
        public Material blitMat;

        [Header("Blit destination options")]
        [Tooltip("blit to CameraTarget,URP CurrentActive,or no, when rendering done")]
        public OutputTarget outputTarget = OutputTarget.CameraTarget;

        [Header("UI options")]
        [Tooltip("ui objects use")]
        public bool isOverrideUIShader;
        [LoadAsset("UI-Default.shader")]
        public Shader overrideUIShader;

        [Header("Render event")]
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
        public int passEventOffset = 10;

        [Header("Filter")]
        [Tooltip("main render object's layer")]
        public FilteringSettingsInfo filterInfo = new FilteringSettingsInfo
        {
            layers = 32,
            renderQueueRangeInfo = new RangeInfo(2501, 5000)
        };

        [Tooltip("render objects use layers, one by one")]
        public List<FilteringSettingsInfo> filterInfoList = new List<FilteringSettingsInfo>();

        [Header("Find Camera by tag")]
        [Tooltip("Define ui camera use this tag, otherwise will check automatic(1 linear space,2 overlay camera,3 camera cullingMask is UI)")]
        public string cameraTag;



        [Header("Fullsize Texture")]
        [Tooltip("create a full size texture,as rendering objects target, otherwise use CameraColor(Depth)Attachment,FSR need this")]
        public bool createFullsizeGammaTex;

        [Tooltip("Need use stencil buffer?")]
        public DepthBufferBits depthBufferBits = DepthBufferBits._24;

        [Header("Override stencil")]
        public StencilStateData stencilStateData;

        [Header("Performance Options")]
        [Tooltip("Best option is close for Middle device.")]
        public bool disableFSR = true;

        [Tooltip("simple rendering flow,No blit,no gamma texture,draw objects,output to camera target")]
        public bool isWriteToCameraTargetDirectly;
        public bool isClearCameraTarget;


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

}