using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [Tooltip("Blit sourceName to targetName")]
    [CreateAssetMenu(menuName=SRP_FEATURE_PASSES_MENU+ "/BlitToTarget")]
    public class BlitToTarget : SRPFeature
    {
        [Header("Source")]
        [Tooltip("When empty will use Current Active")]
        public string sourceName;

        [Header("Target")]
        [Tooltip("When empty will use Camera Target")]
        public string targetName;

        [Tooltip("blit source to [_CameraAttachmentA or B]")]
        public bool isBlitToNextPostTarget;

        [Header("Options")]
        public Material blitMat;

        [Tooltip("use gamma, linear space")]
        public ColorSpaceTransform.ColorSpaceMode colorSpaceMode;

        public override ScriptableRenderPass GetPass() => new BlitToTargetPass(this);
    }

    public class BlitToTargetPass : SRPPass<BlitToTarget>
    {

        public BlitToTargetPass(BlitToTarget feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var cameraTarget = cameraData.targetTexture ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
            RenderTargetIdentifier sourceId = string.IsNullOrEmpty(Feature.sourceName) ? BuiltinRenderTextureType.CurrentActive : Shader.PropertyToID(Feature.sourceName);
            RenderTargetIdentifier targetId = string.IsNullOrEmpty(Feature.targetName) ? cameraTarget : Shader.PropertyToID(Feature.targetName);

            if (Feature.isBlitToNextPostTarget)
            {
                var isA = cameraData.renderer.cameraColorTarget.IsNameIdEquals(ShaderPropertyIds._CameraColorAttachmentA);

                //var isA = colorAttachment == ShaderPropertyIds._CameraColorAttachmentA;
                targetId = isA ? ShaderPropertyIds._CameraColorAttachmentB : ShaderPropertyIds._CameraColorAttachmentA;
            }

            ColorSpaceTransform.SetColorSpace(cmd, Feature.colorSpaceMode);

            if (Feature.blitMat)
                cmd.BlitTriangle(sourceId, targetId, Feature.blitMat, 0);
            else
                cmd.Blit(sourceId, targetId);
        }
    }
}
