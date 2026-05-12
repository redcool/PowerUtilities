#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{

    /// <summary>
    /// for compatible 6000
    /// </summary>
    public partial class SRPPass
    {

        //------------
        public int targetCount;
        public RTHandle depthTarget;
        /// <summary>
        /// {count : RTHandle[]}
        /// </summary>
        public Dictionary<int, RTHandle[]> colorTargetDict = new(){
            {1, new RTHandle[1] },
            {2, new RTHandle[2] },
            {3, new RTHandle[3] },
            {4, new RTHandle[4] },
            {5, new RTHandle[5] },
            {6, new RTHandle[6] },
            {7, new RTHandle[7] },
            {8, new RTHandle[8] },
        };
        public RTClearFlags clearFlags;
        public Color clearColor;
        //================ compatible 2022 fields
        /// <summary>
        /// Configures the camera for rendering, allowing customization of the rendering process before execution.
        /// </summary>
        /// <remarks>Override this method to implement custom camera setup logic. Any modifications to the
        /// rendering data should be compatible with the rendering pipeline in use.</remarks>
        /// <param name="cmd">The command buffer used to record rendering commands for the current camera setup.</param>
        /// <param name="renderingData">A reference to the rendering data that provides information about the current rendering context and
        /// settings.</param>
        public virtual void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) { }
        /// <summary>
        /// Execute render
        /// </summary>
        /// <param name="context"></param>
        /// <param name="renderingData"></param>
        public virtual void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
        public virtual void Configure(CommandBuffer cmd, RenderTextureDescriptor desc) { }
        /// <summary>
        /// All camera render finish
        /// </summary>
        /// <param name="cmd"></param>
        public virtual void OnFinishCameraStackRendering(CommandBuffer cmd) { }

        public void ConfigureTarget(RTHandle colorTarget, RTHandle depthTarget)
        {
            targetCount = 1;
            colorTargetDict[1][0] = colorTarget;
            this.depthTarget = depthTarget;
        }
        public void ConfigureTarget(RTHandle colorTarget)
        {
            targetCount = 1;
            colorTargetDict[1][0] = colorTarget;
            depthTarget = null;
        }
        public void ConfigureTargets(RTHandle[] colorTargets, RTHandle depthTarget)
        {
            var len = colorTargets.Length;
            if (len <= 0) return;
            Array.Copy(colorTargets, colorTargetDict[len], len);
            targetCount = len;
        }

        public void ConfigureClear(ClearFlag flag, Color clearColor)
        {
            this.clearColor = clearColor;
            this.clearFlags = (RTClearFlags)flag;
        }
        public void ConfigureClear(RTClearFlags flag, Color clearColor)
        {
            this.clearColor = clearColor;
            this.clearFlags = flag;
        }
    }

    public class PassData
    {
        public RenderingData legacyRenderingData;
        public UniversalResourceData resourceData;
        public UniversalCameraData cameraData;
        public UniversalShadowData shadowData;
        public UniversalRenderingData renderingData;
        public UniversalPostProcessingData postData;
    }

    public partial class SRPPass<T>
    {
        public enum PassType
        {
            Raster, Compute, Unsafe
        }
        public PassType passType = PassType.Raster;

        public RenderGraph renderGraph;
        public ContextContainer contextContainer;

        /// <summary>
        /// current pass's data
        /// </summary>
        public PassData defaultPassData;

        IRasterRenderGraphBuilder rasterRenderGraphBuilder;

        public void SetupPassData(ref PassData passData, ContextContainer frameData)
        {
            // setup pass data
            passData.legacyRenderingData = Feature.renderingData;
            passData.resourceData = frameData.Get<UniversalResourceData>();
            passData.cameraData = frameData.Get<UniversalCameraData>();
            passData.shadowData = frameData.Get<UniversalShadowData>();
            passData.renderingData = frameData.Get<UniversalRenderingData>();
            passData.postData = frameData.Get<UniversalPostProcessingData>();
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // local fields save in global
            ScriptableRendererEx.renderGraph = renderGraph;
            ScriptableRendererEx.contextContainer = contextContainer;

            // save current pass
            this.renderGraph = renderGraph;
            this.contextContainer = frameData;

            // events
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            RenderPipelineManager.endContextRendering += OnEndContextRendering;

            switch (passType)
            {
                case PassType.Raster: RecordRasterPass(renderGraph, frameData); break;
                case PassType.Compute: RecordComputePass(renderGraph, frameData); break;
                case PassType.Unsafe: RecordUnsafePass(renderGraph, frameData); break;
            }
        }

        private void RecordUnsafePass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddUnsafePass<PassData>(Feature.GetName(), out var passData))
            {
                // setup and save
                SetupPassData(ref passData, frameData);
                defaultPassData = passData;
            }
        }

        private void RecordComputePass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddComputePass<PassData>(Feature.GetName(), out var passData))
            {
                // setup and save
                SetupPassData(ref passData, frameData);
                defaultPassData = passData;
            }
        }

        private void RecordRasterPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(Feature.GetName(), out var passData))
            {
                rasterRenderGraphBuilder = builder;
                // setup and save
                SetupPassData(ref passData, frameData);

                defaultPassData = passData;
                // setup
                OnCameraSetup(CommandBufferEx.defaultCmd, ref Feature.renderingData);
                Configure(CommandBufferEx.defaultCmd, Feature.renderingData.cameraData.cameraTargetDescriptor);

                // builder setup
                builder.AllowPassCulling(false);

                // set pass's targets
                SetRenderTargets(builder, passData);

                //builder.SetRenderAttachment(passData.resourceData.activeColorTexture, 0);
                //builder.SetRenderAttachmentDepth(passData.resourceData.activeDepthTexture);

                // register render callback
                builder.SetRenderFunc((PassData data, RasterGraphContext rasterContext) =>
                {
                    // context is invalid ,skip ,when compile
                    if (context == default)
                        return;

                    context.SetRasterContext(rasterContext);

                    rasterContext.cmd.ClearRenderTarget(clearFlags, clearColor, 1.0f, 0);
                    Execute(context, ref Feature.renderingData);
                });
            }
        }

        private void SetRenderTargets(IRasterRenderGraphBuilder builder, PassData passData)
        {

            // check RenderTargetHolder
            if (RenderTargetHolder.IsLastTargetValid())
            {
                //RenderTargetHolder.GetLastTargets((UniversalRenderer)passData.cameraData.renderer, out RTHandle[] colorRTHArr, out RTHandle depthRTH);
                var colorRTHArr = RenderTargetHolder.LastColorTargetHandles;
                var depthRTH = RenderTargetHolder.LastDepthTargetHandle;
                SetTargets(builder, passData.resourceData, colorRTHArr, depthRTH);
            }
            else
            {
                targetCount = Mathf.Clamp(targetCount, 1, 8);
                var rthArr = colorTargetDict[targetCount];

                SetTargets(builder, passData.resourceData, rthArr, depthTarget);

            }
        }

        private void SetTargets(IRasterRenderGraphBuilder builder, UniversalResourceData resourceData, RTHandle[] colorRTHArr, RTHandle depthRTH)
        {
            var depthTexture = depthRTH != null ? renderGraph.ImportTexture(depthRTH) : resourceData.activeDepthTexture;
            builder.SetRenderAttachmentDepth(depthTexture);

            // use default when null
            if (colorRTHArr[0] == null)
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
            }
            
            for (int i = 0; i < colorRTHArr.Length; i++)
            {
                if (colorRTHArr[i] != null)
                {
                    builder.SetRenderAttachment(renderGraph.ImportTexture(colorRTHArr[i]), i);
                }
            }
        }

        private void OnEndContextRendering(ScriptableRenderContext context, List<Camera> list)
        {
            OnFinishCameraStackRendering(CommandBufferEx.defaultCmd);
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            this.context = context;
            this.camera = camera;
        }

    }

}

#endif