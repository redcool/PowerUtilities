namespace PowerUtilities.SSPR
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public class SSPRSettingSO : ScriptableObject
    {
        [LoadAsset("SSPR.compute")]
        public ComputeShader ssprCS;

        [Header("Options")]
        [Tooltip("texture name for shader")]
        public string reflectionTextureName = "_ReflectionTexture";

        public Vector3 planeLocation = new Vector3(0, 0.01f, 0);
        public Quaternion planeRotation = Quaternion.identity;
        public float fading = 10;

        [Header("Stretch Options")]
        public bool isApplyStretch;
        public float stretchThreshold = 0.95f, stretchIntensity = 1;

        [Header("Key Options")]
        public RunMode runMode;
        public bool runModeAuto = true;
        public bool isFixedHoleInHashMode;

        [Header("control buffer's resolution")]
        [Range(0, 2)] public int downSamples;
        [Header("Blur")]
        public bool isApplyBlur;
        public BlurPassMode blurPassMode;

        [LoadAsset("Hidden_SSPR_Blur.mat")]
        public Material blurMat;
        [Min(1)] public float blurSize = 1;
        [Range(2, 10)] public int stepCount = 10;

        [Header("Render")]
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderEventOffset = 0;
        public string gameCameraTag = "MainCamera";
    }

}