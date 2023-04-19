using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    public class Blit : SRPFeature
    {
        public string sourceName;
        public bool isCurrentActive;
        
        public string targetName;
        public bool isCameraTarget;

        public Material blitMat;

        public override ScriptableRenderPass GetPass() => new BlitPass(this);
    }

    public class BlitPass : SRPPass<Blit>
    {

        public BlitPass(Blit feature) : base(feature) { }

        public override bool IsValid(Camera cam)
        {
            return base.IsValid(cam) || 
                !(string.IsNullOrEmpty(Feature.sourceName) || string.IsNullOrEmpty(Feature.targetName) ||!Feature.isCurrentActive || !Feature.isCameraTarget)
                ;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (!Feature.blitMat)
                return;

            var sourceId = Shader.PropertyToID(Feature.sourceName);
            var targetId = Shader.PropertyToID(Feature.targetName);

            if (Feature.isCurrentActive)
                sourceId = (int)BuiltinRenderTextureType.CurrentActive;
            if(Feature.isCameraTarget)
                targetId = (int) BuiltinRenderTextureType.CameraTarget;

            cmd.BlitTriangle(sourceId, targetId, Feature.blitMat, 0);
        }
    }
}
