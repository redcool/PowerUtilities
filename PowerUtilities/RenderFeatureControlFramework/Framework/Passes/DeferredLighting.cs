using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    public class DeferredLighting : SRPFeature
    {
        [Header("Input")]
        public string gbuffer0;
        public string gbuffer1;
        public string depthBufferName = "_DepthBuffer";
        public Material mat;

        //[Header("Output")]
        //public string targetName;
        public override ScriptableRenderPass GetPass() => new DeferedLightingPass(this);
    }

    public class DeferedLightingPass : SRPPass<DeferredLighting>
    {
        public DeferedLightingPass(DeferredLighting feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() 
                && Feature.mat
                ;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            //RenderTargetIdentifier targetId = string.IsNullOrEmpty(Feature.targetName) ? BuiltinRenderTextureType.CameraTarget : Shader.PropertyToID(Feature.targetName);
            cmd.SetGlobalTexture(ShaderPropertyIds._GBuffer0, Shader.PropertyToID(Feature.gbuffer0));
            cmd.SetGlobalTexture(ShaderPropertyIds._GBuffer1, Shader.PropertyToID(Feature.gbuffer1));
            cmd.SetGlobalTexture(ShaderPropertyIds._CameraDepthTexture, Shader.PropertyToID(Feature.depthBufferName));

        }
    }
}
