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
        public Vector2 fadingRange = new Vector2(5,10);

        [Header("Stretch Options")]
        [Tooltip("Horizontal stretch")]
        public bool isApplyStretch;
        [Tooltip("stretch start from center,[-1,1], 0.5: fixed range [-0.5,0.5]")]
        [Range(0,1)]
        public float stretchThreshold = 0.95f;
        [Tooltip("stretch scale")]
        public float stretchIntensity = 1;

        [Header("Key Options")]
        public RunMode runMode;
        public bool runModeAuto = true;
        [Tooltip("multi sample _HashTexture")]
        public bool isFixedHoleInHashMode = true;

        [Tooltip("control hash texture or buffer's resolution")]
        [Range(0.1f, 1)] public float renderScale = 0.8f;

        [Header("Blur")]
        public bool isApplyBlur;
        public BlurPassMode blurPassMode;
        [Range(0.1f, 1)] public float blurRenderScale = 0.5f;

        [LoadAsset("Hidden_SSPR_Blur.mat")]
        public Material blurMat;
        [Min(1)] public float blurSize = 1;
        [Range(2, 10)] public int stepCount = 10;

        [Header("Render")]
        [Tooltip("urp render pass event")]
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingSkybox;
        public int renderEventOffset = 0;

#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
#endif
        [Tooltip("Which camera run this")]
        public string gameCameraTag = "MainCamera";
    }

}