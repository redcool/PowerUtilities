using System.Collections.Generic;
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
        static int GAMMA_TEX_ID = Shader.PropertyToID("_GammaTex");
        static int _SOURCE_TEX_ID = Shader.PropertyToID("_SourceTex");

        const string _LINEAR_TO_SRGB_CONVERSION = nameof(_LINEAR_TO_SRGB_CONVERSION);
        const string _SRGB_TO_LINEAR_CONVERSION = nameof(_SRGB_TO_LINEAR_CONVERSION);

        RenderTargetIdentifier colorHandleId;
        Material blitMat;
        LayerMask layerMask;

        int gammaTexId;
        bool isGammaTexCreated;
        RenderStateBlock renderStateBlock; // stencil

        CommandBuffer cmd = new CommandBuffer { name=nameof(RenderUIPass) };
        public RenderUIPass(RenderTargetIdentifier handleId, Material blitMat, LayerMask layerMask, StencilState stencilState,int stencilRefValue)
        {
            colorHandleId = handleId;

            this.blitMat = blitMat;
            this.layerMask=layerMask;
            gammaTexId = GAMMA_TEX_ID;

            renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            if (stencilState.enabled)
            {
                renderStateBlock.stencilReference = stencilRefValue;
                renderStateBlock.stencilState = stencilState;
                renderStateBlock.mask = RenderStateMask.Stencil;
            }
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
            cmd.SetGlobalTexture(_SOURCE_TEX_ID, source);
            cmd.SetRenderTarget(destination, colorLoadAction, colorStoreAction, depthLoadAction, depthStoreAction);
            cmd.Blit(source, BuiltinRenderTextureType.CurrentActive, material, passIndex);

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

            RenderTargetIdentifier sourceId = gammaTexId;
            var depthHandleId = "_CameraDepthAttachment";

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            cmd.BeginSample(nameof(RenderUIPass));

            //if (false)
            {
                var desc = cameraData.cameraTargetDescriptor;
                desc.width = cameraData.camera.pixelWidth;
                desc.height = cameraData.camera.pixelHeight;
                cmd.GetTemporaryRT(gammaTexId, desc);

                //---------------------  1 to gamma tex
                blitMat.shaderKeywords=null;
                SetColorSpace(cmd, ColorSpaceTransform.LinearToSRGB);

                //Blit(cmd, colorHandleId, gammaTexId, blitMat);
                cmd.SetGlobalTexture(_SOURCE_TEX_ID, colorHandleId);
                cmd.SetRenderTarget(gammaTexId);
                cmd.Blit(BuiltinRenderTextureType.None, gammaTexId, blitMat);

#if UNITY_ANDROID
                //blit one more time otherwise show nothing
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                {
                    SetColorSpace(cmd, ColorSpaceTransform.None);

                    //Blit(cmd, gammaTexId, colorHandleId, blitMat);
                    cmd.SetGlobalTexture(_SOURCE_TEX_ID, gammaTexId);
                    cmd.SetRenderTarget(colorHandleId);
                    cmd.Blit(BuiltinRenderTextureType.None, colorHandleId, blitMat);
                }
#endif
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }



            //--------------------- 2 draw ui
            {
                var sortFlags = SortingCriteria.CommonTransparent;

                var drawSettings = CreateDrawingSettings(new List<ShaderTagId>{
                new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly"), new ShaderTagId("LightweightForward")
                }, ref renderingData, sortFlags);
                var filterSettings = new FilteringSettings(RenderQueueRange.transparent, layerMask);
#if UNITY_EDITOR
                // When rendering the preview camera, we want the layer mask to be forced to Everything
                if (renderingData.cameraData.isPreviewCamera)
                {
                    filterSettings.layerMask = -1;
                }
#endif

#if UNITY_ANDROID
                // reset depth buffer(depthHandle), otherwise stencil missing
                if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                {
                    cmd.SetRenderTarget(colorHandleId, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store,
                     depthHandleId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
#endif
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref renderStateBlock);
            }



            //--------------------- 3 to colorTarget
            //if (false)
            {
                SetColorSpace(cmd, ColorSpaceTransform.SRGBToLinear);

                Blit(cmd, BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, blitMat);
            }

            //------------- end
            cmd.ReleaseTemporaryRT(gammaTexId);

            SetColorSpace(cmd, ColorSpaceTransform.None);

            cmd.EndSample(nameof(RenderUIPass));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}
