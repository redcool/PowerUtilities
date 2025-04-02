namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class DrawShadowSettingSO : ScriptableObject
    {
        [EditorGroup("ShadowMapOptions", true)]
        public TextureResolution res = TextureResolution.x512;

        [EditorGroup("ShadowMapOptions")]
        [Tooltip("Change _BigShadowMap size, will recreate rt")]
        public bool isOverrideShadowMapRes;

        [EditorGroup("ShadowMapOptions")]
        [ListItemDraw("qualityLevel:,qualityLevel,res:,res", "100,100,50,100")]
        [Tooltip("will override [res] by current qualityLevel and ShadowMapResQualitySettings")]
        public List<BigShadowResQualitySetting> ShadowMapResQualitySettings = new List<BigShadowResQualitySetting>
        {
           new BigShadowResQualitySetting{qualityLevel=0,res=TextureResolution.x256},
           new BigShadowResQualitySetting{qualityLevel=1,res=TextureResolution.x256},
           new BigShadowResQualitySetting{qualityLevel=2,res=TextureResolution.x512},
           new BigShadowResQualitySetting{qualityLevel=3,res=TextureResolution.x512},
           new BigShadowResQualitySetting{qualityLevel=4,res=TextureResolution.x1024},
           new BigShadowResQualitySetting{qualityLevel=5,res=TextureResolution.x1024},
           new BigShadowResQualitySetting{qualityLevel=6,res=TextureResolution.x2048},
        };
         
        [EditorGroup("ShadowMapOptions")]
        [Tooltip("call renderer's ShadowCaster pass, more batch than use override material")]
        public bool isCallShadowCaster;

        [EditorGroup("ShadowMapOptions", intentOffset = 2)]
        [Tooltip("use this material rendering renderer's shadow")]
        [LoadAsset("BigShadowCasterMat.mat")]
        [EditorDisableGroup(targetPropName = nameof(isCallShadowCaster),isRevertMode = true)]
        public Material shadowMat;

        [EditorGroup("ShadowMapOptions")]
        [Tooltip("culling object use layer")]
        public LayerMask layers = 1;

        [EditorGroup("ShadowMapOptions")]
        [RenderingLayerMask(tooltip ="rendering layer mask")]
        public uint renderingLayerMask = int.MaxValue;

        [EditorGroup("ShadowMapOptions")]
        [Tooltip("transparent object use shadowCaster")]
        [EditorDisableGroup(targetPropName = nameof(isPreciseRenderQueue),isRevertMode = true)]
        public bool drawTransparents;

        [EditorGroup("ShadowMapOptions")]
        [Tooltip("Assign precise render queue range for rendering Shadow,opaque: [0-2500],transparent:[2501-5000], all : [0-5000]")]
        public bool isPreciseRenderQueue;

        [EditorGroup("ShadowMapOptions",intentOffset =2)]
        [EditorDisableGroup(targetPropName = nameof(isPreciseRenderQueue))]
        [Tooltip("Queue range,opaque : 0-2500,transparent:2501-5000")]
        public Vector2Int renderQueueRange = new Vector2Int(0,2500);

        //=================================================== Light Camera

        [EditorGroup("Light Camera", true)]
        [Tooltip("Find by tag,disable DrawShadow when lightTransform not found")]
        public bool isUseLightTransform = true;

        [EditorGroup("Light Camera")]
        public string lightTag = "BigShadowLight";

        [EditorGroup("Light Camera")]
        [Tooltip("Use BIgShadowLight BoxCollider's Bounds, auto set (orthoSize,near,far)")]
        public bool isUseBigShadowLightBoxCollider = false;

        //=================================================== Light Camera transform
        [EditorGroup("Light Camera")]
        [EditorHeader("Light Camera", "BigShadowLight Transform")]
        [EditorBox("", "isUsePos,isUseRot,isUseUp", "0.3,0.3,0.3", boxType = EditorBoxAttribute.BoxType.HBox)]
        //[EditorBox("", "isPosEnabled,pos", "0.3,0.3,0.3", boxType = EditorBoxAttribute.BoxType.HBox)]
        [Tooltip("LightCamera use BigShadowLight transform's position")]
        public bool isUsePos = true;

        // dont show these
        [HideInInspector]
        [Tooltip("LightCamera use BigShadowLight transform's rotation")]
        public bool isUseRot = true;

        [HideInInspector]
        [Tooltip("LightCamera use BigShadowLight transform's up")]
        public bool isUseUp = true;

        [EditorGroup("Light Camera")]
        [EditorDisableGroup(targetPropNamesStr = "isUseLightTransform,isUsePos", isRevertMode = true, heightScale = 2)]
        public Vector3 pos;

        [EditorGroup("Light Camera")]
        [EditorDisableGroup(targetPropNamesStr = "isUseLightTransform,isUseRot", isRevertMode = true, heightScale = 2)]
        public Vector3 rot;

        [EditorGroup("Light Camera")]
        [EditorDisableGroup(targetPropNamesStr = "isUseLightTransform,isUseUp", isRevertMode = true, heightScale = 2)]
        public Vector3 up = Vector3.up;

        [EditorGroup("Light Camera")]
        [Tooltip("Offset light position in world space")]
        public Vector3 lightPosOffset;

        //[EditorHeader("Light Camera","Frustum")]
        //=================================================== Light Camera frustum
        [EditorGroup("LightCameraFrustum",true)]
        [Tooltip("half of height")]
        [EditorDisableGroup(targetPropName = "isUseBigShadowLightBoxCollider",isRevertMode =true)]
        public float orthoSize = 20;

        [EditorGroup("LightCameraFrustum")]
        [Tooltip("near clip plane ")]
        [EditorDisableGroup(targetPropName = "isUseBigShadowLightBoxCollider", isRevertMode = true)]
        public float near = 0.3f;

        [EditorGroup("LightCameraFrustum")]
        [Tooltip("far clip plane ")]
        [EditorDisableGroup(targetPropName = "isUseBigShadowLightBoxCollider", isRevertMode = true)]
        public float far = 100;
        //=================================================== Light Camera pos Blend
        [EditorGroup("LightCamera Pos Blend",true)]
        [Tooltip("lightCamera.pos use LightPos or CameraPos")]
        [Range(0,1)]
        public float lightCameraPosBlend = 1;

        //=================================================== Shadow
        [EditorGroup("Shadow", true)]
        [Min(0)] public float shadowDepthBias = 1;

        [EditorGroup("Shadow")]
        [Min(0)] public float shadowNormalBias = 1;

        [EditorGroup("Shadow")]
        [Range(0, 1)] public float shadowIntensity = 1;
        //=================================================== others
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

        [Tooltip("bounds line color")]
        public Color boundsLineColor = Color.gray;
    }
}