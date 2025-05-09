namespace PowerUtilities.RenderFeatures
{
#if UNITY_EDITOR
    using UnityEditor.SceneManagement;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering.Universal.Internal;
    using UnityEngine.SceneManagement;
    using PowerUtilities;
    using Object = UnityEngine.Object;
    using Unity.Mathematics;
#if UNITY_2020
    using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
    using Tooltip = PowerUtilities.TooltipAttribute;
#endif

    [Serializable]
    public class StencilStateData
    {
        /// <summary>
        /// Used to mark whether the stencil values should be overridden or not.
        /// </summary>
        public bool overrideStencilState = false;

        /// <summary>
        /// The stencil reference value.
        /// </summary>
        public int stencilReference = 0;

        /// <summary>
        /// The comparison function to use.
        /// </summary>
        public CompareFunction stencilCompareFunction = CompareFunction.Always;

        /// <summary>
        /// The stencil operation to use when the stencil test passes.
        /// </summary>
        public StencilOp passOperation = StencilOp.Keep;

        /// <summary>
        /// The stencil operation to use when the stencil test fails.
        /// </summary>
        public StencilOp failOperation = StencilOp.Keep;

        /// <summary>
        /// The stencil operation to use when the stencil test fails because of depth.
        /// </summary>
        public StencilOp zFailOperation = StencilOp.Keep;
    }


    [Tooltip("Draw Objects with full urp powers, use SRPBatch or Instanced need multi cameras")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/DrawObjects")]
    public class DrawObjects : SRPFeature
    {
        const string FILTERS_GROUP = "Filters",
            DEBUG_TOOLS_GROUP = "DebugTools",
            REFLECTION_CAMERA_GROUP = "SetupReflectionCameraView",
            CREATE_REFLECTION_CAMERA_GROUP = "CreateReflectionCameraControl"
            ;


        [Header("Draw Objects Options")]
        [EditorBorder]
        [Tooltip("Assign shader tags ,correspond Tags{\"LightMode\"=\"UniversalForward\"}")]
        public string[] shaderTags = new[] {
            "UniversalForwardOnly",
            "UniversalForward",
            "SRPDefaultUnlit"
        };

        //-------------------------------------- FILTERS_GROUP
        [Header("BaseFilters")]
        [EditorBorder(6)]
        [Tooltip("render opaque, opque:[0,2500],transparent:[2501,5000],all:[0,5000]")]
        public RenderQueueType renderQueueType = RenderQueueType.opaque;
        public LayerMask layers = -1;

        [Header("---Override FilterSetting")]
        [Tooltip("use full filterSetting")]
        public bool isOverrideFilterSetting;

        [EditorDisableGroup(targetPropName = "isOverrideFilterSetting")]
        public SimpleFilterSetting filterSetting = new();

        //-------------------------------------- override stencil depth
        [Header("--- Override depth")]
        [EditorBorder()]
        [Tooltip("depth state control")]
        public DepthStateInfo depthState;

        [Header("--- Override stencil")]
        [EditorBorder()]
        [Tooltip("stencil state control")]
        public StencilStateData stencilData;
        //-------------------------------------- override material
        [Header("--- override material")]
        [EditorBorder(3)]
        [Tooltip("use this material render objects when not empty")]
        public Material overrideMaterial;

        [Tooltip("overrideMaterial use pass index")]
        public int overrideMaterialPassIndex;

        [Tooltip("lightMode canot match, use this material")]
        public Material fallbackMaterial;

        //-------------------------------------- per object data
        [Header("--- Per Object Data")]
        [EditorBorder(2)]
        [Tooltip("overridePerObjectData,Lightmap : (Lightmaps,LightProbe,LightProbeProxyVolume),ShadowMask:(ShadowMask,OcclusionProbe,OcclusionProbeProxyVolume)")]
        public bool overridePerObjectData;
        
        [EditorDisableGroup(targetPropName = "overridePerObjectData")]
        public PerObjectData perObjectData;

        //-------------------------------------- override light
        [Header("--- override mainLight")]
        [EditorBorder(5)]
        public bool overrideMainLightIndex;

        [Tooltip("restore mainLightIndex when draw finish")]
        [EditorDisableGroup(targetPropName = "overrideMainLightIndex")]
        public bool isRestoreMainLightIndexFinish = true;

        [EditorDisableGroup(targetPropName = "overrideMainLightIndex")]
        public int mainLightIndex;

        [Tooltip("use this light as mainLight")]
        [EditorDisableGroup(targetPropName = "overrideMainLightIndex")]
        public string lightName;
        [EditorDisableGroup(targetPropName = "overrideMainLightIndex")]
        public List<string> visibleLightNames = new List<string>();

        //-------------------------------------- dynamic batch
        [Header("--- override dynamic batch")]
        [EditorBorder(2)]
        [Tooltip("override urp Pipeline Asset")]
        public bool overrideDynamicBatching;

        [EditorDisableGroup(targetPropName = "overrideDynamicBatching")]
        public bool enableDynamicBatching;

        //-------------------------------------- override instancing
        [Header("--- override instancing")]
        [EditorBorder(2)]
        [Tooltip("override instancing")]
        public bool overrideGPUInstancing;

        [EditorDisableGroup(targetPropName = "overrideGPUInstancing")]
        public bool enableGPUInstancing;

        //-------------------------------------- override srp batch
        [Header("--- override srp batch")]
        [EditorBorder(2)]
        public bool overrideSRPBatch;
        [EditorDisableGroup(targetPropName = "overrideSRPBatch")]
        public bool enableSRPBatch;

        //-------------------------------------- override camera
        [Header("--- override camera")]
        [EditorBorder(3)]
        public bool overrideCamera;

        [EditorDisableGroup(targetPropName = "overrideCamera")]
        public float cameraFOV = 60;
        [EditorDisableGroup(targetPropName = "overrideCamera")]
        public Vector4 cameraOffset;
        //-------------------------------------- override skybox
        [Header("SkyBox Pass")]
        [EditorBorder(1)]
        [Tooltip("skyboxPass use last target,passEvent <= BeforeRenderingSkybox")]
        public bool IsUpdateSkyboxTarget;
        //---------------------DrawChildrenInstanced
        [Header("DrawChildrenInstanced")]
        [EditorBorder(2)]
        public bool isDrawChildrenInstancedOn;

        [EditorDisableGroup(targetPropName = "isDrawChildrenInstancedOn")]
        public bool forceFindDrawChildrenInstanced;

        //---------------------ColorSpace
        [Header("Color Space")]
        [EditorBorder(1)]
        public ColorSpaceTransform.ColorSpaceMode colorSpaceMode;

        //---------------------PlanarReflectionCamera
        [EditorBorder(2,groupName = CREATE_REFLECTION_CAMERA_GROUP)]
        [EditorGroup(CREATE_REFLECTION_CAMERA_GROUP, true)]
        [Tooltip("create planar reflection camera")]
        [EditorButton(onClickCall = "OnCreatePlanarReflectionCamera")]
        public bool isCreatePlanarReflectionCamera;

        //---------------------ReflectionCamera
        [EditorBorder(4,groupName = REFLECTION_CAMERA_GROUP)]
        [EditorGroup(REFLECTION_CAMERA_GROUP, true)]
        [Tooltip("render scene use camera's reflection view matrix")]
        public bool isUseReflectionCamera;

        [EditorGroup(REFLECTION_CAMERA_GROUP)]
        [Tooltip("water plane's height, when reflectionPlaneTr is empty ")]
        public float planeYOffset;

        [EditorGroup(REFLECTION_CAMERA_GROUP)]
        [Tooltip("mirror plane,result mirror effects")]
        public Transform reflectionPlaneTr;

        //---------------------DebugTools
        [EditorBorder(3,groupName = DEBUG_TOOLS_GROUP)]
        [EditorGroup(DEBUG_TOOLS_GROUP, true)]
        [EditorButton()]
        [Tooltip("show additive overdraw mode")]
        public bool isSwitchOverdrawMode;

        [EditorGroup(DEBUG_TOOLS_GROUP)]
        [LoadAsset("SFC_ShowOverdrawAdd.mat")]
        public Material overdrawMat;
        // keep for restore
        [HideInInspector]
        public bool isEnterCheckOverdraw;

        public override ScriptableRenderPass GetPass() => new DrawObjectsPassControl(this);

        void OnCreatePlanarReflectionCamera()
        {
            var planarReflectionGo = Object.FindObjectOfType(typeof(PlanarReflectionCameraControl));
            if (planarReflectionGo == null)
            {
                planarReflectionGo = new GameObject("PlanarReflectionCamera").AddComponent<PlanarReflectionCameraControl>();
            }
#if UNITY_EDITOR
            UnityEditor.EditorGUIUtility.PingObject(planarReflectionGo);
#endif
        }
    }

    public class DrawObjectsPassControl : SRPPass<DrawObjects>
    {
        
        FullDrawObjectsPass drawObjectsPass;

        DrawChildrenInstancedPass drawChildrenInstancedPass;

        public DrawObjectsPassControl(DrawObjects feature) : base(feature)
        {
            drawObjectsPass = new FullDrawObjectsPass(feature);
            drawChildrenInstancedPass = new DrawChildrenInstancedPass(feature);
        }

        public override bool IsTryRestoreLastTargets(Camera c) => c.IsGameCamera();

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            drawObjectsPass.OnCameraSetup(cmd, ref renderingData);

            if (Feature.isDrawChildrenInstancedOn)
                drawChildrenInstancedPass.OnCameraSetup(cmd, ref renderingData);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref CameraData cameraData = ref renderingData.cameraData;

            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;

            // reset skybox 's target in gameview
            if(Feature.IsUpdateSkyboxTarget)
                DrawSkyBoxPass.SetupURPSkyboxTargets(renderer,camera);

            // draw scene use reflection camera(view,proj)
            var viewMat = cameraData.GetViewMatrix();
            var projMat = cameraData.GetProjectionMatrix();
            if (Feature.isUseReflectionCamera)
                SetupRenderReflectionCamera(ref cameraData,cmd,viewMat,projMat);
            // draw scene
            drawObjectsPass.OnExecute(context, ref renderingData, cmd);

            if (Feature.isDrawChildrenInstancedOn)
                drawChildrenInstancedPass.OnExecute(context, ref renderingData, cmd);

            // restore camera (view,proj)
            if (Feature.isUseReflectionCamera)
                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMat, projMat, false);
        }



        private void SetupRenderReflectionCamera(ref CameraData cameraData, CommandBuffer cmd, Matrix4x4 viewMat, Matrix4x4 projMat)
        {
            var camTr = cameraData.camera.transform;
            camTr.GetReflection(Feature.reflectionPlaneTr, Feature.planeYOffset, out var camForward, out var camUp, out var camPos);

            var v = Float4x4Ex.LookAtInverse(camPos, camPos + camForward, camUp);
            DebugTools.DrawAxis(camPos, v.c0.xyz, v.c1.xyz, v.c2.xyz);

            RenderingUtils.SetViewAndProjectionMatrices(cmd, v, projMat, false);
        }
    }

    public class DrawChildrenInstancedPass : SRPPass<DrawObjects>
    {
        DrawChildrenInstanced[] drawChildren;
        public int findCount;

        public DrawChildrenInstancedPass(DrawObjects feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (Feature.forceFindDrawChildrenInstanced)
            {
                Feature.forceFindDrawChildrenInstanced = false;
                findCount = 0;
            }
            //drawChildren.ForEach(dc =>
            for (int i = 0; i < drawChildren.Length; i++)
            {
                var dc = drawChildren[i];
                dc.drawInfoSO.DrawGroupList(cmd);
                cmd.Execute(ref context);
            }
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            if (findCount <1)
            {
                findCount ++;
#if UNITY_2022_1_OR_NEWER
                drawChildren = Object.FindObjectsByType<DrawChildrenInstanced>(FindObjectsSortMode.None);
#else
                drawChildren = Object.FindObjectsOfType<DrawChildrenInstanced>();
#endif
            }


        }
    }

    public class FullDrawObjectsPass : SRPPass<DrawObjects>
    {
        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");
        public List<ShaderTagId> shaderTagList = new List<ShaderTagId>();

        public FilteringSettings filteringSettings;
        public RenderStateBlock renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

        public static event RefAction<DrawingSettings> OnSetupDrawSettings;

        Light sun;
        bool lastSRPBatchEnabled;

        public FullDrawObjectsPass(DrawObjects feature) : base(feature)
        {
            Init();
        }
        /// <summary>
        /// Apply Feature data
        /// when change feature's params call this
        /// </summary>
        public void Init()
        {
            SetupShaderTagList(Feature.shaderTags);
            SetupFilterSettings(Feature);
            SetupRenderStateBlock(Feature);
        }

        public void SetupShaderTagList(string[] shaderTags)
        {
            shaderTagList.Clear();
            foreach (string shaderTag in shaderTags)
                shaderTagList.Add(new ShaderTagId(shaderTag));
        }

        public void SetupRenderStateBlock(DrawObjects feature)
        {
            // setup render stateBlock(depth,stencil)
            var stencilData = feature.stencilData;
            var stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            if (feature.stencilData.overrideStencilState)
            {
                renderStateBlock.stencilState = stencilState;
                renderStateBlock.mask = RenderStateMask.Stencil;
                renderStateBlock.stencilReference = stencilData.stencilReference;
            }

            if (feature.depthState.isOverrideDepthState)
            {
                renderStateBlock.mask |= RenderStateMask.Depth;
                renderStateBlock.depthState = new DepthState(feature.depthState.isWriteDepth, feature.depthState.compareFunc);
            }
        }

        public void SetupFilterSettings(DrawObjects feature)
        {
            // setup filterSettings
            var renderQueueRange = RenderQueueTools.ToRenderQueueRange(feature.renderQueueType);
            filteringSettings = new FilteringSettings(renderQueueRange, feature.layers);

            if (feature.isOverrideFilterSetting)
                filteringSettings = feature.filterSetting;
        }

        private FilteringSettings GetFilterSettings()
        {
            if (filteringSettings.layerMask != Feature.layers)
                filteringSettings.layerMask = Feature.layers;

            return filteringSettings;
        }

        void SwitchCheckOverdraw()
        {
            if (Feature.isSwitchOverdrawMode)
            {
                Feature.isSwitchOverdrawMode = false;

                // switch overdraw mode
                Feature.isEnterCheckOverdraw = !Feature.isEnterCheckOverdraw;

                if (Feature.isEnterCheckOverdraw)
                {
                    EnterCheckOverdrawMode();
                }
                else
                {
                    ExistCheckOverdrawMode();
                }
            }
            
            // inner methods
            void ExistCheckOverdrawMode()
            {
                Feature.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
                Feature.overrideMaterial = null;
            }

            void EnterCheckOverdrawMode()
            {
                Feature.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
                Feature.overrideMaterial = Feature.overdrawMat;
            }
        }


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd,ref renderingData);

            sun = RenderSettings.sun;

            lastSRPBatchEnabled = UniversalRenderPipeline.asset.useSRPBatcher;

            SwitchCheckOverdraw();
        }

        static NativeArray<RenderStateBlock> renderStateBlockArr;

        //[ApplicationExit]
        //[CompileStarted]
        static void DisposeNative()
        {
            if (renderStateBlockArr.IsCreated)
                renderStateBlockArr.Dispose();
        }

        static FullDrawObjectsPass()
        {
            ApplicationTools.OnDomainUnload += DisposeNative;
        }

        void DrawScene(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var drawObjectPassData = new Vector4(0, 0, 0, Feature.renderQueueType == RenderQueueType.opaque ? 1 : 0);
            cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);

            // scaleBias.x = flipSign
            // scaleBias.y = scale
            // scaleBias.z = bias
            // scaleBias.w = unused
            var flipSign = renderingData.cameraData.IsCameraProjectionMatrixFlipped() ? -1f : 1f;
            var scaleBias = flipSign < 0 ? new Vector4(flipSign, 1, -1, 1) : new Vector4(flipSign, 0, 1, 1);
            cmd.SetGlobalVector(ShaderPropertyIds.scaleBias, scaleBias);
            ColorSpaceTransform.SetColorSpace(cmd,Feature.colorSpaceMode);
            cmd.Execute(ref context);

            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            var filterSetting = GetFilterSettings();
#if UNITY_EDITOR
            var scene = SceneManager.GetActiveScene();
            var stage = StageUtility.GetCurrentStage();

            var isPrefabStage = !string.IsNullOrEmpty(stage.assetPath);
            if (camera.cameraType == CameraType.Preview || (isPrefabStage&&Feature.isShowAllInPrefabStage))
                filterSetting.layerMask = -1;

#endif
            OverrideCamera(ref context, cmd, ref renderingData);
            var drawSettings = GetDrawSettings(context, cmd, ref renderingData, ref cameraData);

            NativeArrayTools.CreateIfNull(ref renderStateBlockArr, 1);
            renderStateBlockArr[0] = renderStateBlock;

            context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSetting, null, renderStateBlockArr);

            RenderingTools.DrawErrorObjects(cmd,ref context, ref renderingData.cullResults, camera, filterSetting, SortingCriteria.None);

            RestoreDrawSettings(ref renderingData, cmd);
        }
        
        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            DrawScene(context,ref renderingData, cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (Feature.overrideSRPBatch)
            {
                UniversalRenderPipeline.asset.useSRPBatcher = lastSRPBatchEnabled;
            }
        }

        private void RestoreDrawSettings(ref RenderingData renderingData, CommandBuffer cmd)
        {
            var cameraData = renderingData.cameraData;

            if (Feature.overrideMainLightIndex && Feature.isRestoreMainLightIndexFinish)
            {
                OverrideMainLight(context, ref renderingData, sun);
            }


            if (Feature.overrideCamera)
            {
                RenderingUtils.SetViewAndProjectionMatrices(cmd, cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix(), false);
            }
        }

        private DrawingSettings GetDrawSettings(ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData, ref CameraData cameraData)
        {
            var cam = renderingData.cameraData.camera;

            var sortFlags = Feature.renderQueueType == RenderQueueType.opaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            var drawSettings = CreateDrawingSettings(shaderTagList, ref renderingData, sortFlags);
            drawSettings.overrideMaterial = Feature.overrideMaterial;
            drawSettings.overrideMaterialPassIndex = Feature.overrideMaterialPassIndex;
#if UNITY_2021_1_OR_NEWER
            drawSettings.fallbackMaterial = Feature.fallbackMaterial;
#endif
            if (Feature.overrideMainLightIndex)
            {
                // show current vl name
                Feature.visibleLightNames.Clear();
                Feature.visibleLightNames.AddRange(
                    renderingData.lightData.visibleLights
                    .Where(vl => vl.lightType == LightType.Directional)
                    .Select(vl => vl.light.name)
                    );

                Feature.mainLightIndex = Mathf.Clamp(Feature.mainLightIndex, 0, renderingData.lightData.visibleLights.Length-1);
                var mainLightIndex = Feature.mainLightIndex;
                //// find by lightName
                if (!string.IsNullOrEmpty(Feature.lightName))
                {
                    mainLightIndex = FindLightIndex(ref renderingData, Feature.lightName);
                }

                OverrideMainLight(context, ref renderingData, renderingData.lightData.visibleLights[mainLightIndex].light);
            }
            if (Feature.overridePerObjectData)
            {
                drawSettings.perObjectData = Feature.perObjectData;
            }

            if (Feature.overrideDynamicBatching)
                drawSettings.enableDynamicBatching = Feature.enableDynamicBatching;

            if (Feature.overrideGPUInstancing)
                drawSettings.enableInstancing = Feature.enableGPUInstancing;

            if (Feature.overrideSRPBatch && UniversalRenderPipeline.asset)
            {

                UniversalRenderPipeline.asset.useSRPBatcher = Feature.enableSRPBatch;
                GraphicsSettings.useScriptableRenderPipelineBatching = Feature.enableSRPBatch;
            }

            if (OnSetupDrawSettings != null)
            {
                OnSetupDrawSettings(ref drawSettings);
            }

            // disable instancing
            if (cameraData.isPreviewCamera)
                drawSettings.enableInstancing = false;

            return drawSettings;
        }

        private void OverrideCamera(ref ScriptableRenderContext context, CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (Feature.overrideCamera)
            {
                var cameraData = renderingData.cameraData;
                var cam = cameraData.camera;

                var aspect = cam.pixelWidth / (float)cam.pixelHeight;
                var projMat = Matrix4x4.Perspective(Feature.cameraFOV, aspect, cam.nearClipPlane, cam.farClipPlane);
                projMat = GL.GetGPUProjectionMatrix(projMat, renderingData.cameraData.IsCameraProjectionMatrixFlipped());

                var viewMat = cameraData.GetViewMatrix();
                viewMat.SetColumn(3, viewMat.GetColumn(3) + Feature.cameraOffset);

                RenderingUtils.SetViewAndProjectionMatrices(cmd, viewMat, projMat, false);

                cmd.Execute(ref context);
            }
        }

        public int FindLightIndex(ref RenderingData renderingData, string lightGOName)
        {
            var id = 0;
            foreach (var vl in renderingData.lightData.visibleLights)
            {
                if (vl.light.name == lightGOName)
                    return id;
                id++;
            }
            return -1;
        }

        private void OverrideMainLight(ScriptableRenderContext context, ref RenderingData renderingData, Light mainLight)
        {
            RenderSettings.sun = mainLight;
        }
    }

}
