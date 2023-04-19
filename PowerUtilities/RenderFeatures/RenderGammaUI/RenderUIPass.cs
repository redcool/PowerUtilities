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



        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

#if UNITY_EDITOR
            if (cameraData.isSceneViewCamera)
            {
                DrawRenderers(ref context, ref renderingData, 2,2);
                return;
            }
#endif

            cmd.BeginSample(nameof(RenderUIPass));

            int colorHandleId,depthHandleId;
            SetupTargetTex(ref renderingData, ref cameraData, out colorHandleId,out depthHandleId);

            //---------------------  1 to gamma tex
            BlitToGammaTarget(ref context,ref cameraData,ref renderingData, colorHandleId,depthHandleId);


            //--------------------- 2 draw ui
            DrawRenderers(ref context, ref renderingData, depthHandleId, colorHandleId);


            //--------------------- 3 to colorTarget

            SetColorSpace(cmd, ColorSpaceTransform.SRGBToLinear);
            Blit(cmd, BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, settings.blitMat);

            //------------- end 
            if (settings.createFullsizeGammaTex)
                cmd.ReleaseTemporaryRT(_GammaTex);

            SetColorSpace(cmd, ColorSpaceTransform.None);
            
            cmd.EndSample(nameof(RenderUIPass));

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        void BlitToGammaTarget(ref ScriptableRenderContext context,ref CameraData cameraData,ref RenderingData renderingData,int colorHandleId,int depthHandleId)
        {
            settings.blitMat.shaderKeywords=null;
            SetColorSpace(cmd, ColorSpaceTransform.LinearToSRGB);

            //BlitToTarget(cmd, colorHandleId, gammaTexId, blitMat);
            // _CameraOpaqueTexture is _CameraColorAttachmentA or _CameraColorAttachmentB
            var lastTargetId = GetLastColorTargetId(ref renderingData);
            cmd.SetGlobalTexture(_SourceTex, lastTargetId);
            cmd.SetRenderTarget(colorHandleId);

            if (settings.createFullsizeGammaTex)
            {
                /**  copy depth from _CameraDepthTexture
                 * 
                cmd.SetRenderTarget(depthHandleId);
                //cmd.BlitToTarget(BuiltinRenderTextureType.None, depthHandleId, copyDepthMat);

                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, copyDepthMat);
                cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
                */
                cmd.ClearRenderTarget(true, false, Color.black, 1);
            }

            cmd.Blit(BuiltinRenderTextureType.None, colorHandleId, settings.blitMat);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        private void DrawRenderers(ref ScriptableRenderContext context, ref RenderingData renderingData, int depthHandleId, int targetTexId)
        {
            var sortFlags = SortingCriteria.CommonTransparent;

            var drawSettings = CreateDrawingSettings(new List<ShaderTagId>{
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"), 
                new ShaderTagId("UniversalForwardOnly"), 
                new ShaderTagId("LightweightForward")
                }, ref renderingData, sortFlags);
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
                //cmd.SetRenderTarget(targetTexId);
                cmd.SetRenderTarget(targetTexId, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                 depthHandleId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
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
        private void SetupTargetTex(ref RenderingData renderingData, ref CameraData cameraData, out int colorHandleId,out int depthHandleId)
        {
            /** =============================================
             * 
             * hacking, use target name direct, when urp upgrade this will not work maybe.
             * 
             * =============================================
             * **/
            depthHandleId = _CameraDepthAttachment;

            colorHandleId = GetLastColorTargetId(ref renderingData);

            if (settings.createFullsizeGammaTex)
            {
                //var desc = cameraData.cameraTargetDescriptor;
                var desc = new RenderTextureDescriptor(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight);
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.Default;
                desc.sRGB = false;
                if (!settings.useStencilBuffer)
                {
                    desc.depthBufferBits = 0;
                }
                else
                {
                    desc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt;
                }

                cmd.GetTemporaryRT(_GammaTex, desc);

                colorHandleId = _GammaTex;
                depthHandleId = _GammaTex;
            }
        }
    }
}
