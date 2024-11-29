using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities.RenderFeatures
{

    [Tooltip("Blit sourceName to targetName")]
    [CreateAssetMenu(menuName=SRP_FEATURE_PASSES_MENU+ "/BlitToTarget")]
    public class BlitToTarget : SRPFeature
    {
        [EditorHeader("","--- Source")]
        [Tooltip("When empty will use Current Active")]
        public string sourceName;
        public bool isShowCurMainLightShadowMapTexture;

        [EditorHeader("","--- Target")]
        [Tooltip("When empty will use Camera Target")]
        public string targetName;

        [Tooltip("blit source to [_CameraAttachmentA or B],if sourceId is A, targetId will set B")]
        public bool isBlitToNextPostTarget;

        [EditorHeader("", "--- Options")]
        // PowerShaderLib/CopyColor.mat
        [Tooltip("check (CopyColor.mat,CopyDepth.mat)")]
        public Material blitMat;

        [Tooltip("use gamma, linear space")]
        public ColorSpaceTransform.ColorSpaceMode colorSpaceMode;

        public BlendMode srcMode = BlendMode.One;
        public BlendMode dstMode = BlendMode.Zero;
        public ClearFlag clearFlags = ClearFlag.None;

        public override ScriptableRenderPass GetPass() => new BlitToTargetPass(this);

        public BlitToTarget()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        }
    }

    public class BlitToTargetPass : SRPPass<BlitToTarget>
    {

        public BlitToTargetPass(BlitToTarget feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;

            RenderTargetIdentifier sourceId = default;
            // === check shadowMap,currentActive, temporary target
            if (Feature.isShowCurMainLightShadowMapTexture)
            {
                sourceId = ShadowUtilsEx.currentMainLightShadowMapTexture;
            }
            else
            {
                sourceId = string.IsNullOrEmpty(Feature.sourceName) ? (RenderTargetIdentifier)BuiltinRenderTextureType.CurrentActive : Shader.PropertyToID(Feature.sourceName);
            }

            var cameraTarget = camera.GetCameraTarget();
            RenderTargetIdentifier targetId = string.IsNullOrEmpty(Feature.targetName) ? cameraTarget : Shader.PropertyToID(Feature.targetName);

            // === check urp target
#if UNITY_2022_1_OR_NEWER
            renderer.TryReplaceURPRTTarget(Feature.sourceName,ref sourceId);
            renderer.TryReplaceURPRTTarget(Feature.targetName, ref targetId);
#endif
            
            // === check rt dict
            if(RenderTextureTools.TryGetRT(Feature.sourceName,out var rt))
                sourceId = rt;
            if (RenderTextureTools.TryGetRT(Feature.targetName, out rt))
                targetId = rt;

            // === check post target(A,B)
            if (Feature.isBlitToNextPostTarget)
            {
#if UNITY_2022_1_OR_NEWER
                var attachmentA = renderer.GetCameraColorAttachmentA();
                var attachmentB = renderer.GetCameraColorAttachmentB();

                //var currentActive = renderer.GetActiveCameraColorAttachment();
                targetId = (sourceId.IsNameIdEquals(attachmentA.nameID)) ? attachmentB : attachmentA;
#else
                var isA = sourceId.IsNameIdEquals(ShaderPropertyIds._CameraColorAttachmentA);
                targetId = isA ? ShaderPropertyIds._CameraColorAttachmentB : ShaderPropertyIds._CameraColorAttachmentA;
#endif
            }
            
            ColorSpaceTransform.SetColorSpace(cmd, Feature.colorSpaceMode);

            if (Feature.blitMat)
                cmd.BlitTriangle(sourceId, targetId, Feature.blitMat, 0, 
                    finalSrcMode: Feature.srcMode, 
                    finalDstMode: Feature.dstMode, 
                    clearFlags: Feature.clearFlags);
            else
                cmd.Blit(sourceId, targetId);

            ColorSpaceTransform.SetColorSpace(cmd,  ColorSpaceTransform.ColorSpaceMode.None);
        }
    }
}
