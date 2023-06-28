using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName=SRP_FEATURE_PASSES_MENU+ "/BlitToTarget")]
    public class BlitToTarget : SRPFeature
    {
        [Header("Source")]
        [Tooltip("When empty will use Current Active")]
        public string sourceName;

        [Header("Target")]
        [Tooltip("When empty will use Camera Target")]
        public string targetName;

        [Header("Options")]
        public Material blitMat;

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

            if (Feature.blitMat)
                cmd.BlitTriangle(sourceId, targetId, Feature.blitMat, 0);
            else
                cmd.Blit(sourceId, targetId);
        }
    }
}
