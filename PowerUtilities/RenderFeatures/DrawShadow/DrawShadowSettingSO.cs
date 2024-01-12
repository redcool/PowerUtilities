namespace PowerUtilities
{
    using System;
    using UnityEngine;

    [Serializable]
    public class DrawShadowSettingSO : ScriptableObject
    {
        [EditorGroup("ShadowMapOptions", true)]
        public TextureResolution res = TextureResolution.x512;

        [EditorGroup("ShadowMapOptions")]
        [Tooltip("call renderer's ShadowCaster pass, more batch than use override material")]
        public bool isCallShadowCaster;

        [EditorGroup("ShadowMapOptions")]
        [Tooltip("use override material,dont use ShadowCaster,will cause more srp batches!")]
        [LoadAsset("BigShadowCasterMat.mat")]
        public Material shadowMat;

        [EditorGroup("ShadowMapOptions")]
        public LayerMask layers = 0;

        [EditorGroup("ShadowMapOptions")]
        public bool drawTransparents;

        [EditorGroup("Light Camera", true)]
        [Tooltip("Find by tag,disable DrawShadow when lightTransform not found")]
        public bool isUseLightTransform = true;

        [EditorGroup("Light Camera")]
        public string lightTag = "BigShadowLight";

        [EditorGroup("Light Camera")]
        public Vector3 pos, rot, up = Vector3.up;

        [EditorGroup("Light Camera")]
        [Tooltip("light position'sy in world space")]
        public float lightHeight = 10;


        [EditorGroup("Light Camera")]
        [Tooltip("half of height")]
        public float orthoSize = 20;

        [EditorGroup("Light Camera")]
        [Tooltip("near clip plane ")]
        public float near = 0.3f;

        [EditorGroup("Light Camera")]
        [Tooltip("far clip plane ")]
        public float far = 100;

        [EditorGroup("Shadow", true)]
        [Min(0)] public float shadowDepthBias = 1;

        [EditorGroup("Shadow")]
        [Min(0)] public float shadowNormalBias = 1;

        [EditorGroup("Shadow")]
        [Range(0, 1)] public float shadowIntensity = 1;

        [Header("Render control")]
        [Tooltip("draw shadow frame then stop,when isAutoRendering = false")]
        [EditorButton] public bool isStepRender = true;

        [Tooltip("draw shadow per frame")]
        public bool isAutoRendering;
        [Tooltip("Follow camera, when distance > maxDistance, draw shadow once,<=0 disable Follow Camera")]
        public float maxDistance = -1;

        // light position finally.
        [HideInInspector] public Vector3 finalLightPos;

        [Tooltip("false will set _BigShadowMap white tex")]
        [EditorButton] public bool isClearShadowMap;
    }
}