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
        public RTHandle depthTarget;
        /// <summary>
        /// max 8 target
        /// </summary>
        public RTHandle[] colorTargets = new RTHandle[8];
        /// <summary>
        /// [1-8]
        /// </summary>
        public int targetCount;

        // clear options
        public RTClearFlags clearFlags;
        public Color clearColor;
        public float clearDepth = 1;
        public uint clearStencil = 0;

        /// <summary>
        /// current pass's data
        /// </summary>
        public SFCPassData defaultPassData;

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
            colorTargets[0] = colorTarget;
            this.depthTarget = depthTarget;
        }
        public void ConfigureTarget(RTHandle colorTarget)
        {
            targetCount = 1;
            colorTargets[0] = colorTarget;
            depthTarget = null;
        }
        /// <summary>
        /// Save targets to dict, and set target count, then will set targets in render callback
        /// </summary>
        /// <param name="colorTargets"></param>
        /// <param name="depthTarget"></param>
        public void ConfigureTargets(RTHandle[] colorTargets, RTHandle depthTarget)
        {
            var len = colorTargets.Length;
            if (len <= 0) return;
            targetCount = Mathf.Clamp(len, 1, 8);

            for (int i = 0; i < len; i++)
            {
                this.colorTargets[i] = colorTargets[i];
            }
        }

        public void ConfigureClear(ClearFlag flag, Color clearColor)
        {
            this.clearColor = clearColor;
            this.clearFlags = (RTClearFlags)flag;
        }
        public void ConfigureClear(RTClearFlags flag, Color clearColor,float depth=1,uint stencil=0)
        {
            this.clearColor = clearColor;
            this.clearFlags = flag;
            this.clearDepth = depth;
            this.clearStencil = stencil;
        }
    }

    public class SFCPassData
    {
        public RenderingData legacyRenderingData;
        public UniversalResourceData resourceData;
        public UniversalCameraData cameraData;
        public UniversalShadowData shadowData;
        public UniversalRenderingData renderingData;
        public UniversalPostProcessingData postData;
        public UniversalLightData lightData;

        public RenderGraph renderGraph;
        public ContextContainer contextContainer;
        //public IBaseRenderGraphBuilder baseBuilder;

        /// <summary>
        /// rasterBuilder in RecordRenderGraph, only valid in render callback, will be set in RecordRenderGraph
        /// </summary>
        public IRasterRenderGraphBuilder rasterBuilder;
        /// <summary>
        /// computeBuilder in RecordRenderGraph, only valid in render callback, will be set in RecordRenderGraph
        /// </summary>
        public IComputeRenderGraphBuilder computeBuilder;
        /// <summary>
        /// unsafeBuilder in RecordRenderGraph, only valid in render callback, will be set in RecordRenderGraph
        /// </summary>
        public IUnsafeRenderGraphBuilder unsafeBuilder;

        public RasterGraphContext RasterContext => ScriptableRenderContextEx_6000.GetRasterContext(default);
        public ComputeGraphContext computeContext => ScriptableRenderContextEx_6000.GetComputeContext(default);
        public UnsafeGraphContext unsafeContext => ScriptableRenderContextEx_6000.GetUnsaftContext(default);

        // current color target for inside RenderFunc
        public TextureHandle colorTargetHandle;

        public SRPPass srpPass;
        public SRPFeature srpFeature;
    }

    public partial class SRPPass<T>
    {
        public enum PassType
        {
            Raster, Compute, Unsafe
        }
        public PassType passType = PassType.Raster;


        public void SetupPassData(ref SFCPassData passData, ContextContainer frameData, RenderGraph rg)
        {
            // setup pass data
            passData.legacyRenderingData = Feature.renderingData;
            passData.resourceData = frameData.Get<UniversalResourceData>();
            passData.cameraData = frameData.Get<UniversalCameraData>();
            passData.shadowData = frameData.Get<UniversalShadowData>();
            passData.renderingData = frameData.Get<UniversalRenderingData>();
            passData.postData = frameData.Get<UniversalPostProcessingData>();
            passData.lightData = frameData.Get<UniversalLightData>();
            passData.renderGraph = rg;

            passData.srpFeature = Feature;
            passData.srpPass = this;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
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
            using (var builder = renderGraph.AddUnsafePass<SFCPassData>(Feature.GetName(), out var passData))
            {
                // setup and save
                SetupPassData(ref passData, frameData, renderGraph);
                passData.unsafeBuilder = builder;
                defaultPassData = passData;
            }
        }

        private void RecordComputePass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddComputePass<SFCPassData>(Feature.GetName(), out var passData))
            {
                // setup and save
                SetupPassData(ref passData, frameData, renderGraph);
                passData.computeBuilder = builder;
                defaultPassData = passData;
                builder.SetRenderFunc((SFCPassData data, ComputeGraphContext computeContext) =>
                {

                });
            }
        }

        private void RecordRasterPass(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<SFCPassData>(Feature.GetName(), out var passData))
            {
                // setup and save
                SetupPassData(ref passData, frameData, renderGraph);
                passData.rasterBuilder = builder;

                defaultPassData = passData;
                // setup
                OnCameraSetup(CommandBufferEx.defaultCmd, ref Feature.renderingData);
                Configure(CommandBufferEx.defaultCmd, Feature.renderingData.cameraData.cameraTargetDescriptor);

                // builder setup
                builder.AllowPassCulling(false);

                // set pass's targets
                SetRenderTargets();
                passData.colorTargetHandle = passData.resourceData.activeColorTexture;

                // register render callback
                builder.SetRenderFunc(static (SFCPassData data, RasterGraphContext rasterContext) =>
                {
                    // keep compatible with 2022, get flip info from CameraData_6000,
                    CameraData_6000.textureUVOrigin = rasterContext.GetTextureUVOrigin(data.colorTargetHandle);


                    // context is invalid ,skip ,when compile
                    if (data.srpPass.context == default)
                        return;

                    //=========== before Execute

                    // save rasterContext to contextContainer, for user to get in Execute
                    data.srpPass.context.SetRasterContext(rasterContext);

                    if (data.srpPass.clearFlags != RTClearFlags.None)
                        rasterContext.cmd.ClearRenderTarget(data.srpPass.clearFlags, data.srpPass.clearColor, data.srpPass.clearDepth, data.srpPass.clearStencil);

                    //=========== call Execute
                    data.srpPass.Execute(data.srpPass.context, ref data.srpFeature.renderingData);
                });
            }
        }

        private void SetRenderTargets()
        {
            // check RenderTargetHolder
            if (camera && IsTryRestoreLastTargets(camera)
                && RenderTargetHolder.IsLastTargetValid())
            {
                var colorRTHArr = RenderTargetHolder.LastColorTargetHandles;
                var depthRTH = RenderTargetHolder.LastDepthTargetHandle;
                var targetCount = RenderTargetHolder.colorTargetNames.Length;

                SetDepthTarget(depthRTH);
                SetColorTargets(colorRTHArr, targetCount);
            }
            else
            {
                SetDepthTarget(depthTarget);
                SetColorTargets(colorTargets, targetCount);
            }
        }

        public TextureHandle GetTextureHandle(RTHandle rth)
        {
            return defaultPassData.renderGraph.ImportTexture(rth);
        }
        
        private void SetColorTargets(RTHandle[] rtHandles, int colorTargetCount)
        {
            for (int i = 0; i < colorTargetCount; i++)
            {
                if (rtHandles[i] != null && rtHandles[i].rt)
                {
                    defaultPassData.rasterBuilder.SetRenderAttachment(GetTextureHandle(rtHandles[i]), i);
                }
            }

            // use default when null
            if (rtHandles[0] == null)
            {
                defaultPassData.rasterBuilder.SetRenderAttachment(defaultPassData.resourceData.activeColorTexture, 0);
            }
        }

        public void SetDepthTarget(RTHandle depthRTH)
        {
            var depthTexture = depthRTH != null ? GetTextureHandle(depthRTH) : defaultPassData.resourceData.activeDepthTexture;
            defaultPassData.rasterBuilder.SetRenderAttachmentDepth(depthTexture);
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