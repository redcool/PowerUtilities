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

        Material blitMat;
        LayerMask layerMask;

        RenderStateBlock renderStateBlock; // stencil

        CommandBuffer cmd = new CommandBuffer { name=nameof(RenderUIPass) };

        public bool createFullsizeTex;
        public RenderUIPass(Material blitMat, LayerMask layerMask, StencilState stencilState,int stencilRefValue)
        {
            this.blitMat = blitMat;
            this.layerMask=layerMask;

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
            BlitToGammaTarget(ref context,ref cameraData, colorHandleId,depthHandleId);


            //--------------------- 2 draw ui
            DrawRenderers(ref context, ref renderingData, depthHandleId, colorHandleId);


            //--------------------- 3 to colorTarget

            SetColorSpace(cmd, ColorSpaceTransform.SRGBToLinear);
            Blit(cmd, BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, blitMat);

            //------------- end 
            if (createFullsizeTex)
                cmd.ReleaseTemporaryRT(_GammaTex);

            SetColorSpace(cmd, ColorSpaceTransform.None);
            
            cmd.EndSample(nameof(RenderUIPass));

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        void BlitToGammaTarget(ref ScriptableRenderContext context,ref CameraData cameraData,int colorHandleId,int depthHandleId)
        {
            blitMat.shaderKeywords=null;
            SetColorSpace(cmd, ColorSpaceTransform.LinearToSRGB);

            //Blit(cmd, colorHandleId, gammaTexId, blitMat);
            cmd.SetGlobalTexture(_SourceTex, _CameraOpaqueTexture); // _CameraOpaqueTexture is _CameraColorAttachmentA or _CameraColorAttachmentB
            cmd.SetRenderTarget(colorHandleId);

            if (createFullsizeTex)
            {
                /**  copy depth from _CameraDepthTexture
                 * 
                cmd.SetRenderTarget(depthHandleId);
                //cmd.Blit(BuiltinRenderTextureType.None, depthHandleId, copyDepthMat);

                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, copyDepthMat);
                cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
                */
                cmd.ClearRenderTarget(true, false, Color.black, 1);
            }

            cmd.Blit(BuiltinRenderTextureType.None, colorHandleId, blitMat);

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
            var filterSettings = new FilteringSettings(RenderQueueRange.transparent, layerMask);
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

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref renderStateBlock);
        }

        bool AnyCameraHasPostProcessing()
        {
            return Camera.allCameras.
                Select(c => c.GetComponent<UniversalAdditionalCameraData>()).
                Any(cd => cd.renderPostProcessing);
        }

        private void SetupTargetTex(ref RenderingData renderingData, ref CameraData cameraData, out int colorHandleId,out int depthHandleId)
        {
            /** =============================================
             * 
             * hacking, use target name direct, when urp upgrade this will not work maybe.
             * 
             * =============================================
             * **/
            depthHandleId = _CameraDepthAttachment;

            colorHandleId = AnyCameraHasPostProcessing() && renderingData.postProcessingEnabled ? _CameraColorAttachmentA : _CameraColorAttachmentB;

            if (createFullsizeTex)
            {
                var desc = cameraData.cameraTargetDescriptor;
                //desc.msaaSamples = 1;
                desc.width = cameraData.camera.pixelWidth;
                desc.height = cameraData.camera.pixelHeight;
                cmd.GetTemporaryRT(_GammaTex, desc);

                colorHandleId = _GammaTex;
                depthHandleId = _GammaTex;
            }
        }
    }
}
