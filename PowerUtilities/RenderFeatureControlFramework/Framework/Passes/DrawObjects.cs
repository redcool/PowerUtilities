namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering.Universal.Internal;

    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/DrawObjects")]
    public class DrawObjects : SRPFeature
    {
        [Header("Draw Objects Options")]
        public string[] shaderTags = new[] {
            "UniversalForwardOnly",
            "UniversalForward",
            "SRPDefaultUnlit"
        };
        [Tooltip("render opaque(queue<=2000) or transparent(queue>=3000),need match rebderPassEvent")]
        public bool isOpaque = true;
        public LayerMask layers = -1;

        [Header("Override")]
        [Tooltip("stencil state conttro")]
        public StencilStateData stencilData;

        [Tooltip("use this material render objects when not empty")]
        public Material overrideMaterial;

        [Tooltip("overrideMaterial use pass index")]
        public int overrideMaterialPassIndex;

        [Tooltip("lightMode canot match, use this material")]
        public Material fallbackMaterial;

        [Space(10)]
        [Tooltip("overridePerObjectData,Lightmap : (Lightmaps,LightProbe,LightProbeProxyVolume)" +
            ",ShadowMask:(ShadowMask,OcclusionProbe,OcclusionProbeProxyVolume)")]
        public bool overridePerObjectData;
        public PerObjectData perObjectData;

        [Space(10)]
        public bool overrideMainLightIndex;
        [Tooltip("restore mainLightIndex when draw finish")]
        public bool isRestoreMainLightIndexFinish=true;
        
        public int mainLightIndex;

        [Tooltip("use this light as mainLight")]
        public string lightName;
        public List<string> visibleLightNames = new List<string>();

        [Space(10)]
        [Tooltip("override urp Pipeline Asset")]
        public bool overrideDynamicBatching;
        public bool enableDynamicBatching;

        [Space(10)]
        [Tooltip("override instancing")]
        public bool overrideGPUInstancing;
        public bool enableGPUInstancing;

        [Space(10)]
        public bool overrideSRPBatch;
        public bool enableSRPBatch;
        [Tooltip("restore URPPipelineAsset's useSRPBatch when finish")]
        public bool isRestoreSRPBatch = true;

        [Header("SkyBox")]
        public bool isDrawSkybox;
        public RenderPassEvent drawSkyboxEvent = RenderPassEvent.BeforeRenderingSkybox;

        public override ScriptableRenderPass GetPass() => new DrawObjectsPassWrapper(this);
    }

    public class DrawObjectsPassWrapper : SRPPass<DrawObjects>
    {
        //DrawObjectsPass 
        FullDrawObjectsPass
            drawObjectsPass;

        DrawSkyboxPass drawSkyboxPass;
        public DrawObjectsPassWrapper(DrawObjects feature) : base(feature)
        {
            //drawObjectsPass = GetDrawObjectsPass(feature);

            drawObjectsPass = new FullDrawObjectsPass(feature);

            if (feature.isDrawSkybox)
                drawSkyboxPass = new DrawSkyboxPass(feature.drawSkyboxEvent);
        }

        public static DrawObjectsPass GetUrpDrawObjectsPass(DrawObjects feature)
        {
            UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;

            var stencilData = feature.stencilData;
            var stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            var shaderTagIds = feature.shaderTags
                .Select(name => new ShaderTagId(name))
                .ToArray();

            var renderQueueRange = feature.isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;

            return new DrawObjectsPass(feature.name, shaderTagIds, feature.isOpaque, feature.renderPassEvent, renderQueueRange, feature.layers, stencilState, stencilData.stencilReference);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            drawObjectsPass.OnCameraSetup(cmd, ref renderingData);

            if (drawSkyboxPass != null)
                drawSkyboxPass.OnCameraSetup(cmd, ref renderingData);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            cmd.BeginSampleExecute(Feature.name, ref context);

            drawObjectsPass.Execute(context, ref renderingData);

            if (drawSkyboxPass != null)
                drawSkyboxPass.Execute(context, ref renderingData);

            cmd.EndSampleExecute(Feature.name, ref context);
        }
    }

    public class FullDrawObjectsPass : SRPPass<DrawObjects>
    {
        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");
        public List<ShaderTagId> shaderTagList = new List<ShaderTagId>();

        public FilteringSettings filteringSettings;
        public RenderStateBlock renderStateBlock;

        public static event RefAction<DrawingSettings> OnSetupDrawSettings;

        ForwardLights forwardLights;

        int lastMainLightIndex;
        bool lastSRPBatchEnabled;

        public FullDrawObjectsPass(DrawObjects feature) : base(feature)
        {
            shaderTagList.AddRange(feature.shaderTags.Select(n => new ShaderTagId(n)));
            var renderQueueRange = feature.isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;
            filteringSettings = new FilteringSettings(renderQueueRange, feature.layers);

            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

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

        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            lastMainLightIndex = renderingData.lightData.mainLightIndex;
            lastSRPBatchEnabled = UniversalRenderPipeline.asset.useSRPBatcher;

            if (renderStateBlock.depthState.compareFunction == CompareFunction.Equal)
            {
                renderStateBlock.depthState = new DepthState(true, CompareFunction.LessEqual);
                renderStateBlock.mask |= RenderStateMask.Depth;
            }
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {

            var drawObjectPassData = new Vector4(0, 0, 0, Feature.isOpaque ? 1 : 0);
            cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);

            // scaleBias.x = flipSign
            // scaleBias.y = scale
            // scaleBias.z = bias
            // scaleBias.w = unused
            var flipSign = renderingData.cameraData.IsCameraProjectionMatrixFlipped() ? -1f : 1f;
            var scaleBias = flipSign < 0 ? new Vector4(flipSign, 1, -1, 1) : new Vector4(flipSign, 0, 1, 1);
            cmd.SetGlobalVector(ShaderPropertyIds.scaleBias, scaleBias);
            cmd.Execute(ref context);

            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;


            var filterSetting = filteringSettings;
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.Preview)
                filterSetting.layerMask = -1;
#endif

            var drawSettings = GetDrawSettings(context,ref renderingData, ref cameraData);
            drawSettings = CreateDrawingSettings(shaderTagList, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSetting, ref renderStateBlock);

            RenderingTools.DrawErrorObjects(ref context, ref renderingData.cullResults, camera, filterSetting, SortingCriteria.None);

            RestoreDrawSettings(ref renderingData);

        }

        private void RestoreDrawSettings(ref RenderingData renderingData)
        {
            if (Feature.overrideMainLightIndex && Feature.isRestoreMainLightIndexFinish)
            {
                OverrideLight(context, ref renderingData, lastMainLightIndex);
            }
            if (Feature.overrideSRPBatch && Feature.isRestoreSRPBatch)
            {
                UniversalRenderPipeline.asset.useSRPBatcher = lastSRPBatchEnabled;
            }
        }

        private DrawingSettings GetDrawSettings(ScriptableRenderContext context, ref RenderingData renderingData, ref CameraData cameraData)
        {
            var sortFlags = Feature.isOpaque ? cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            var drawSettings = CreateDrawingSettings(shaderTagList, ref renderingData, sortFlags);
            drawSettings.overrideMaterial = Feature.overrideMaterial;
            drawSettings.overrideMaterialPassIndex = Feature.overrideMaterialPassIndex;
            drawSettings.fallbackMaterial = Feature.fallbackMaterial;

            if (Feature.overrideMainLightIndex)
            {
                // show current vl name
                Feature.visibleLightNames.Clear();
                Feature.visibleLightNames.AddRange(
                    renderingData.lightData.visibleLights
                    .Where(vl => vl.lightType == LightType.Directional)
                    .Select(vl => vl.light.name)
                    );

                var mainLightIndex = Feature.mainLightIndex;
                //// find by lightName
                if (!string.IsNullOrEmpty(Feature.lightName))
                {
                    mainLightIndex = FindLightIndex(ref renderingData, Feature.lightName);
                }

                OverrideLight(context, ref renderingData, mainLightIndex);
            }

            if (Feature.overrideDynamicBatching)
                drawSettings.enableDynamicBatching = Feature.enableDynamicBatching;

            if (Feature.overrideGPUInstancing)
                drawSettings.enableInstancing = Feature.enableGPUInstancing;

            if (Feature.overrideSRPBatch && UniversalRenderPipeline.asset)
                UniversalRenderPipeline.asset.useSRPBatcher = Feature.enableSRPBatch;

            if (OnSetupDrawSettings != null)
            {
                OnSetupDrawSettings(ref drawSettings);
            }

            // disable instancing
            if (cameraData.isPreviewCamera)
                drawSettings.enableInstancing = false;

            return drawSettings;
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

        private void OverrideLight(ScriptableRenderContext context, ref RenderingData renderingData,int mainLightIndex)
        {
            renderingData.lightData.mainLightIndex = mainLightIndex;

            if (forwardLights == null)
                forwardLights = new ForwardLights();

            forwardLights.Setup(context, ref renderingData);
        }
    }

}
