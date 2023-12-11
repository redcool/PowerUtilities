using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Features
{

    public class RenderUIPass : ScriptableRenderPass
    {
        static readonly int _GammaTex = Shader.PropertyToID(nameof(_GammaTex));

        static readonly int _SourceTex = Shader.PropertyToID(nameof(_SourceTex));
        static readonly int _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture));
        static readonly int _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture));
        static readonly int _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA));
        static readonly int _CameraColorAttachmentB = Shader.PropertyToID(nameof(_CameraColorAttachmentB));
        static readonly int _CameraDepthAttachment = Shader.PropertyToID(nameof(_CameraDepthAttachment));

        CommandBuffer cmd = new CommandBuffer { name=nameof(RenderUIPass) };

        public RenderGammaUIFeature.Settings settings;

        StencilState GetStencilState()
        {
            var stencilState = new StencilState();
            stencilState.SetCompareFunction(settings.stencilStateData.stencilCompareFunction);
            stencilState.SetFailOperation(settings.stencilStateData.failOperation);
            stencilState.SetPassOperation(settings.stencilStateData.passOperation);
            stencilState.SetZFailOperation(settings.stencilStateData.zFailOperation);
            stencilState.enabled = settings.stencilStateData.overrideStencilState;
            return stencilState;
        }

        RenderStateBlock GetRenderStateBlock()
        {
            var stencilState = GetStencilState();
            var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            if (stencilState.enabled)
            {
                renderStateBlock.stencilReference = settings.stencilStateData.stencilReference;
                renderStateBlock.stencilState = stencilState;
                renderStateBlock.mask = RenderStateMask.Stencil;
            }
            return renderStateBlock;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var urpAsset = UniversalRenderPipeline.asset;
            var isUseFSR = urpAsset.upscalingFilter == UpscalingFilterSelection.FSR;
            if(isUseFSR && settings.disableFSR)
            {
                urpAsset.upscalingFilter = UpscalingFilterSelection.Auto;
            }

            if(settings.useSRPBatcher != urpAsset.useSRPBatcher)
                urpAsset.useSRPBatcher = settings.useSRPBatcher;
        }

        bool IsWriteToCameraTargetDirect()
        {
            if (Display.main.requiresSrgbBlitToBackbuffer)
                return true;

            return settings.isWriteToCameraTargetDirectly;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            cmd.BeginSampleExecute(nameof(RenderUIPass),ref context);
            Draw(ref context, ref renderingData);
            cmd.EndSampleExecute(nameof(RenderUIPass),ref context);
        }
        public void Draw(ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

            // ------- wrtie to cameraTarget
            if (IsWriteToCameraTargetDirect())
            {
                ClearDefaultCameraDepth(ref context,cmd);

                if (Display.main.requiresSrgbBlitToBackbuffer)
                {
                    ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.LinearToSRGB);
                }

                DrawRenderers(ref context, ref renderingData, BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
                return;
            }

#if UNITY_EDITOR
            if (cameraData.isSceneViewCamera)
            {
                DrawRenderers(ref context, ref renderingData, BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
                return;
            }
#endif
            // ------ gamma ui flow

            RenderTargetIdentifier lastColorHandleId, colorHandleId, depthHandleId;
            SetupTargetTex(ref renderingData, ref cameraData, out lastColorHandleId, out colorHandleId, out depthHandleId);

            //---------------------  1 to gamma tex
            settings.blitMat.shaderKeywords = null;
            //settings.blitMat.SetFloat("_Double", 0);

            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.LinearToSRGB);
            BlitToTarget(ref context, lastColorHandleId, colorHandleId, depthHandleId);

            //--------------------- 2 draw ui
            DrawRenderers(ref context, ref renderingData, colorHandleId, depthHandleId);


            //--------------------- 3 to colorTarget
            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.SRGBToLinear);

            if (settings.isFinalRendering)
            {
                // write to CameraTarget
                cmd.BlitTriangle(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, settings.blitMat, 0);
            }
            else
            {
                // write to urp target
                BlitToTarget(ref context, colorHandleId, lastColorHandleId, depthHandleId);
            }

            //------------- end 
            if (settings.createFullsizeGammaTex)
                cmd.ReleaseTemporaryRT(_GammaTex);

            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.None);

            cmd.Execute(ref context);
        }

        void ClearDefaultCameraDepth(ref ScriptableRenderContext context,CommandBuffer cmd)
        {
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd.ClearRenderTarget(true, false, Color.clear);
            cmd.Execute(ref context);
        }

        void BlitToTarget(ref ScriptableRenderContext context, RenderTargetIdentifier lastColorHandleId, RenderTargetIdentifier colorHandleId, RenderTargetIdentifier depthHandleId)
        {
            // _CameraOpaqueTexture is _CameraColorAttachmentA or _CameraColorAttachmentB
            cmd.SetGlobalTexture(_SourceTex, lastColorHandleId);
            cmd.SetRenderTarget(colorHandleId,depthHandleId);

            if (settings.createFullsizeGammaTex)
            {
                cmd.ClearRenderTarget(true, false, Color.black,1);
            }

            //cmd.Blit(BuiltinRenderTextureType.None, colorHandleId, settings.blitMat); // will set _MainTex
            cmd.BlitTriangle(lastColorHandleId, colorHandleId, settings.blitMat, 0);
        }

        private void DrawRenderers(ref ScriptableRenderContext context, ref RenderingData renderingData, RenderTargetIdentifier targetTexId, RenderTargetIdentifier depthHandleId)
        {
            var sortFlags = SortingCriteria.CommonTransparent;

            var drawSettings = CreateDrawingSettings(RenderingTools.urpForwardShaderPassNames, ref renderingData, sortFlags);

            if (settings.isOverrideUIShader && settings.UIMaterial)
                drawSettings.overrideMaterial = settings.UIMaterial;

            //var filterSettings = new FilteringSettings(RenderQueueRange.transparent, settings.filterInfo.layers);
            FilteringSettings filterSettings = settings.filterInfo;
#if UNITY_EDITOR
            // When rendering the preview camera, we want the layer mask to be forced to Everything
            if (renderingData.cameraData.isPreviewCamera)
            {
                filterSettings.layerMask = -1;
            }
#endif

            // reset depth buffer(depthHandle), otherwise depth&stencil are missing
            {
                //cmd.SetRenderTarget(targetTexId, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                // depthHandleId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

                cmd.SetRenderTarget(targetTexId, depthHandleId);

                cmd.Execute(ref context);
            }

            var renderStateBlock = GetRenderStateBlock();
            var arr = new NativeArray<RenderStateBlock>(new[] { renderStateBlock }, Allocator.Temp);
            context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings, null, arr);

            // render objects by layerMasks order
            if (settings.filterInfoList.Count >0)
            {
                foreach (var info in settings.filterInfoList)
                {
                    if (!info.enabled)
                        continue;
                    //filterSettings.layerMask = info.layers;
                    //filterSettings.renderQueueRange = new RenderQueueRange(info.renderQueueRangeInfo.min, info.renderQueueRangeInfo.max);

                    //FilteringSettingsInfo.SetupFilterSettingss(ref filterSettings, info);
                    filterSettings = info;

                    context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings, null, arr);
                }
            }
        }

        bool AnyCameraHasPostProcessing()
        {
            //return Camera.allCameras.
            //    Select(c => c.GetComponent<UniversalAdditionalCameraData>()).
            //    Any(cd => cd.renderPostProcessing);
            foreach (var cam in Camera.allCameras)
            {
                var cd = cam.GetComponent<UniversalAdditionalCameraData>();
                if(cd && cd.renderPostProcessing) return true;
            }
            return false;
        }

        RenderTargetIdentifier m_ActiveCameraColorAttachment, colorAttachmentA, colorAttachmentB;
        
        private void SetupTargetTex(ref RenderingData renderingData, ref CameraData cameraData,out RenderTargetIdentifier lastColorHandleId, out RenderTargetIdentifier colorHandleId, out RenderTargetIdentifier depthHandleId)
        {
            var renderer = (UniversalRenderer)cameraData.renderer;
            lastColorHandleId = renderer.GetRenderTargetId(URPRTHandleNames.m_ActiveCameraColorAttachment);
            colorAttachmentA = renderer.GetRenderTargetId(URPRTHandleNames._CameraColorAttachmentA);
            colorAttachmentB = renderer.GetRenderTargetId(URPRTHandleNames._CameraColorAttachmentB);
            depthHandleId = renderer.GetRenderTargetId(URPRTHandleNames.m_CameraDepthAttachment);

            colorHandleId = lastColorHandleId == colorAttachmentA ? colorAttachmentB : colorAttachmentA;

            if (settings.createFullsizeGammaTex)
            {
                //var desc = cameraData.cameraTargetDescriptor;
                var desc = new RenderTextureDescriptor(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight);
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.Default;
                desc.sRGB = false;
                desc.depthBufferBits = (int)settings.depthBufferBits;
                cmd.GetTemporaryRT(_GammaTex, desc);

                colorHandleId = _GammaTex;
                depthHandleId = _GammaTex;
            }
        }
    }
}
