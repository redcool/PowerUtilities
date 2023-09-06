using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Features
{
    public enum ColorSpaceTransform
    {
        None,LinearToSRGB,SRGBToLinear
    }
    public class RenderUIPass : ScriptableRenderPass
    {
        static readonly int _GammaTex = Shader.PropertyToID(nameof(_GammaTex));

        static readonly int _SourceTex = Shader.PropertyToID(nameof(_SourceTex));
        static readonly int _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture));
        static readonly int _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture));
        static readonly int _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA));
        static readonly int _CameraColorAttachmentB = Shader.PropertyToID(nameof(_CameraColorAttachmentB));
        static readonly int _CameraDepthAttachment = Shader.PropertyToID(nameof(_CameraDepthAttachment));

        const string _LINEAR_TO_SRGB_CONVERSION = nameof(_LINEAR_TO_SRGB_CONVERSION);
        const string _SRGB_TO_LINEAR_CONVERSION = nameof(_SRGB_TO_LINEAR_CONVERSION);

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

        void SetColorSpace(CommandBuffer cmd, ColorSpaceTransform trans)
        {
            switch (trans)
            {
                case ColorSpaceTransform.LinearToSRGB:
                    cmd.EnableShaderKeyword(_LINEAR_TO_SRGB_CONVERSION);
                    cmd.DisableShaderKeyword(_SRGB_TO_LINEAR_CONVERSION);
                    break;

                case ColorSpaceTransform.SRGBToLinear:
                    cmd.EnableShaderKeyword(_SRGB_TO_LINEAR_CONVERSION);
                    cmd.DisableShaderKeyword(_LINEAR_TO_SRGB_CONVERSION);
                    break;

                default:
                    cmd.DisableShaderKeyword(_SRGB_TO_LINEAR_CONVERSION);
                    cmd.DisableShaderKeyword(_LINEAR_TO_SRGB_CONVERSION);
                    break;
            }
        }

        void Blit(CommandBuffer cmd,
                    RenderTargetIdentifier source,
                    RenderTargetIdentifier destination,
                    Material material,
                    int passIndex = 0,
                    RenderBufferLoadAction colorLoadAction = RenderBufferLoadAction.Load,
                    RenderBufferStoreAction colorStoreAction = RenderBufferStoreAction.Store,
                    RenderBufferLoadAction depthLoadAction = RenderBufferLoadAction.Load,
                    RenderBufferStoreAction depthStoreAction = RenderBufferStoreAction.Store)
        {
            cmd.SetGlobalTexture(_SourceTex, source);
            cmd.SetRenderTarget(destination, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
            cmd.Blit(source, BuiltinRenderTextureType.CurrentActive, material, passIndex);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var isUseFSR = UniversalRenderPipeline.asset.upscalingFilter == UpscalingFilterSelection.FSR;
            if(isUseFSR && settings.disableFSR)
            {
                UniversalRenderPipeline.asset.upscalingFilter = UpscalingFilterSelection.Auto;
            }
        }

        bool IsWriteToCameraTargetDirect()
        {
            if (Display.main.requiresSrgbBlitToBackbuffer)
                return true;

            return settings.isWriteToCameraTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            cmd.SetGlobalFloat(ShaderPropertyIds._GUIZTestMode, (int)CompareFunction.Always);

            if (IsWriteToCameraTargetDirect())
            {
                if (Display.main.requiresSrgbBlitToBackbuffer)
                {
                    SetColorSpace(cmd, ColorSpaceTransform.LinearToSRGB);
                }
                //cmd.SetGlobalFloat(ShaderPropertyIds._GUIZTestMode, (int)CompareFunction.Always);
                DrawRenderers(ref context, ref renderingData, 2, 2);
                return;
            }

#if UNITY_EDITOR
            if (cameraData.isSceneViewCamera)
            {
                DrawRenderers(ref context, ref renderingData, 2,2);
                return;
            }
#endif

            cmd.BeginSample(nameof(RenderUIPass));

            int lastColorHandleId,colorHandleId,depthHandleId;
            SetupTargetTex(ref renderingData, ref cameraData, out lastColorHandleId, out colorHandleId,out depthHandleId);

            //---------------------  1 to gamma tex
            settings.blitMat.shaderKeywords=null;
            //settings.blitMat.SetFloat("_Double", 0);

            SetColorSpace(cmd, ColorSpaceTransform.LinearToSRGB);
            BlitToGammaTarget(ref context, lastColorHandleId, colorHandleId, depthHandleId);

            //--------------------- 2 draw ui
            DrawRenderers(ref context, ref renderingData, colorHandleId,depthHandleId);


            //--------------------- 3 to colorTarget
            SetColorSpace(cmd, ColorSpaceTransform.SRGBToLinear);
            Blit(cmd, BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, settings.blitMat);

            //------------- end 
            if (settings.createFullsizeGammaTex)
                cmd.ReleaseTemporaryRT(_GammaTex);

            SetColorSpace(cmd, ColorSpaceTransform.None);
            
            cmd.EndSample(nameof(RenderUIPass));

            cmd.Execute(ref context);
        }

        void BlitToGammaTarget(ref ScriptableRenderContext context, int lastColorHandleId, RenderTargetIdentifier colorHandleId, RenderTargetIdentifier depthHandleId)
        {
            // _CameraOpaqueTexture is _CameraColorAttachmentA or _CameraColorAttachmentB
            cmd.SetGlobalTexture(_SourceTex, lastColorHandleId);
            cmd.SetRenderTarget(colorHandleId,depthHandleId);

            if (settings.createFullsizeGammaTex)
            {
                //cmd.ClearRenderTarget(true, false, Color.black, 1);
            }

            cmd.Blit(BuiltinRenderTextureType.None, colorHandleId, settings.blitMat);
        }

    private void DrawRenderers(ref ScriptableRenderContext context, ref RenderingData renderingData, RenderTargetIdentifier targetTexId,RenderTargetIdentifier depthHandleId)
        {
            var sortFlags = SortingCriteria.CommonTransparent;

            var drawSettings = CreateDrawingSettings(RenderingTools.urpForwardShaderPassNames, ref renderingData, sortFlags);
            
            if(settings.isOverrideUIShader && settings.UIMaterial)
                drawSettings.overrideMaterial = settings.UIMaterial;

            var filterSettings = new FilteringSettings(RenderQueueRange.transparent, settings.layerMask);
#if UNITY_EDITOR
            // When rendering the preview camera, we want the layer mask to be forced to Everything
            if (renderingData.cameraData.isPreviewCamera)
            {
                filterSettings.layerMask = -1;
            }
#endif

            // reset depth buffer(depthHandle), otherwise depth&stencil are missing
            {
                cmd.SetRenderTarget(targetTexId, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                 depthHandleId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

                cmd.Execute(ref context);
            }

            var renderStateBlock = GetRenderStateBlock();
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref renderStateBlock);
        }

        bool AnyCameraHasPostProcessing()
        {
            return Camera.allCameras.
                Select(c => c.GetComponent<UniversalAdditionalCameraData>()).
                Any(cd => cd.renderPostProcessing);
        }

        int GetLastColorTargetId(ref RenderingData renderingData) => (AnyCameraHasPostProcessing() && renderingData.postProcessingEnabled )? _CameraColorAttachmentB : _CameraColorAttachmentA ;
        private void SetupTargetTex(ref RenderingData renderingData, ref CameraData cameraData,out int lastColorHandleId, out int colorHandleId, out int depthHandleId)
        {

            /** =============================================
             * 
             * hacking, use target name direct, when urp upgrade this will not work maybe.
             * 
             * =============================================
             * **/
            depthHandleId = _CameraDepthAttachment;

            lastColorHandleId = GetLastColorTargetId(ref renderingData);
            colorHandleId = (lastColorHandleId == _CameraColorAttachmentA) ? _CameraColorAttachmentB : _CameraColorAttachmentA;

            if (settings.createFullsizeGammaTex)
            {
                //var desc = cameraData.cameraTargetDescriptor;
                var desc = new RenderTextureDescriptor(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight);
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.Default;
                desc.sRGB = false;
                desc.depthBufferBits = settings.useStencilBuffer ? 24 : 0;
                cmd.GetTemporaryRT(_GammaTex, desc);

                colorHandleId = _GammaTex;
                depthHandleId = _GammaTex;
            }
        }
    }
}
