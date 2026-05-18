using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;
#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif
namespace PowerUtilities.RenderFeatures
{

#if UNITY_6000_4_OR_NEWER
    public class TestSRPPass : SRPPass<TestSRPFeature>
    {
        RendererListHandle opaqueList;
        RendererListHandle transparentList;
        RendererListHandle skyboxList;

        RenderStateBlock stateBlock = new();
        NativeArray<RenderStateBlock> stateBlockArr;

        NativeArray<ShaderTagId> tagValueArr;

        public TestSRPPass(TestSRPFeature feature) : base(feature) { }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            var shaderTags = new[] {
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward")
            };
            var drawSettings = new DrawingSettings();
            for (int i = 0; i < shaderTags.Length; i++)
            {
                drawSettings.SetShaderPassName(i, shaderTags[i]);
            }
            var sortSettings = new SortingSettings(camera);
            sortSettings.criteria = SortingCriteria.CommonOpaque;

            drawSettings.sortingSettings = sortSettings;
            drawSettings.perObjectData = renderingData.perObjectData;
            drawSettings.mainLightIndex = renderingData.lightData.mainLightIndex;

            drawSettings.overrideMaterial = Feature.isOverrideMode ? Feature.overrideMat : null;

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            filterSettings.layerMask = Feature.layers;
            filterSettings.renderingLayerMask = Feature.renderingLayers;

            SetupStateBlock();
            NativeArrayTools.CreateIfNull(ref stateBlockArr, 1);
            stateBlockArr[0] = stateBlock;
            NativeArrayTools.CreateIfNull(ref tagValueArr, 1);
            tagValueArr[0] = shaderTags[0];

            var opaqueListParams = new RendererListParams(renderingData.cullResults,drawSettings,filterSettings)
            {
                stateBlocks = stateBlockArr,
                tagValues = tagValueArr
            };

            opaqueList = defaultPassData.renderGraph.CreateRendererList(opaqueListParams);

            // skybox
            skyboxList = defaultPassData.renderGraph.CreateSkyboxRendererList(camera);

            // transparent
            sortSettings.criteria = SortingCriteria.CommonTransparent;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            var transparentListParams = new RendererListParams(renderingData.cullResults, drawSettings, filterSettings);

            transparentList = defaultPassData.renderGraph.CreateRendererList(transparentListParams);

            // set rendererlist
            defaultPassData.rasterBuilder.UseRendererList(opaqueList);
            defaultPassData.rasterBuilder.UseRendererList(skyboxList);
            defaultPassData.rasterBuilder.UseRendererList(transparentList);

            // set target
            //defaultPassData.rasterBuilder.SetRenderAttachment(defaultPassData.resourceData.activeColorTexture,0);
            //defaultPassData.rasterBuilder.SetRenderAttachmentDepth(defaultPassData.resourceData.activeDepthTexture);
        }

        private void SetupStateBlock()
        {
            // stencil
            var stencilState = StencilState.defaultValue;
            stencilState.SetCompareFunction(CompareFunction.Always);
            stencilState.SetPassOperation(StencilOp.Replace);
            stencilState.SetFailOperation(StencilOp.Keep);
            stencilState.SetZFailOperation(StencilOp.Keep);
            // depth
            var depthState = DepthState.defaultValue;
            depthState.compareFunction = CompareFunction.LessEqual;
            depthState.writeEnabled = true;
            // raster
            var rasterState = RasterState.defaultValue;
            rasterState.cullingMode = CullMode.Back;

            //blend
            var blendState = BlendState.defaultValue;
            blendState.blendState0 = new RenderTargetBlendState(sourceColorBlendMode: BlendMode.SrcAlpha, destinationColorBlendMode: BlendMode.OneMinusSrcAlpha);

            stateBlock.mask = RenderStateMask.Nothing;
            stateBlock.mask |= RenderStateMask.Stencil;
            stateBlock.mask |= RenderStateMask.Depth;
            stateBlock.mask |= RenderStateMask.Raster;
            stateBlock.mask |= RenderStateMask.Blend;

            stateBlock.stencilState = stencilState;
            stateBlock.depthState = depthState;
            stateBlock.rasterState = rasterState;
            stateBlock.blendState = blendState;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;
            ref var cameraData = ref renderingData.cameraData;
            var desc = cameraData.cameraTargetDescriptor;

            defaultPassData.RasterContext.cmd.DrawRendererList(opaqueList);
            defaultPassData.RasterContext.cmd.DrawRendererList(skyboxList);
            defaultPassData.RasterContext.cmd.DrawRendererList(transparentList);
        }
    }
#endif // 
}
