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
        int GAMMA_TEX_ID = Shader.PropertyToID("_FULLSIZE_GAMMA_TEX");
        int _SOURCE_TEX_ID = Shader.PropertyToID("_SourceTex");

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
        public void Setup(int gammaTexId)
        {
            this.gammaTexId = gammaTexId;
            isGammaTexCreated = true;
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

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            cmd.BeginSample(nameof(RenderUIPass));

            //if (!isGammaTexCreated)
            {
                var desc = cameraData.cameraTargetDescriptor;
                desc.width = cameraData.camera.pixelWidth;
                desc.height = cameraData.camera.pixelHeight;
                cmd.GetTemporaryRT(gammaTexId, desc);

                //---------------------  1 to gamma tex
                blitMat.shaderKeywords=null;
                SetColorSpace(cmd, ColorSpaceTransform.LinearToSRGB);

                //RenderingUtils.Blit(cmd, colorHandleId, gammaTexId, blitMat);
                cmd.SetGlobalTexture(_SOURCE_TEX_ID, colorHandleId);
                cmd.Blit(colorHandleId, gammaTexId, blitMat);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }

            //--------------------- 2 draw ui
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

            //cmd.SetRenderTarget(gammaTexId);
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref renderStateBlock);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();


            //--------------------- 3 to framebuffer
            SetColorSpace(cmd, ColorSpaceTransform.SRGBToLinear);

            //RenderingUtils.Blit(cmd, gammaTexId, BuiltinRenderTextureType.CameraTarget,blitMat);
            cmd.SetGlobalTexture(_SOURCE_TEX_ID, gammaTexId);
            cmd.Blit(gammaTexId, BuiltinRenderTextureType.CameraTarget, blitMat);
            // need.
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            //------------- end
            cmd.ReleaseTemporaryRT(gammaTexId);

            SetColorSpace(cmd, ColorSpaceTransform.None);

            cmd.EndSample(nameof(RenderUIPass));
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }
    }
}
