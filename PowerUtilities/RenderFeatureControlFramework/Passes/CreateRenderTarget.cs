using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_MENU+"/CreateRenderTarget")]
    public class CreateRenderTarget : SRPFeature
    {
        public string[] colorTargetNames;
        public bool isTargetHasDepthBuffer;
        public bool isHDR;

        public string depthTargetName;
        [Range(0.1f,2)]public float renderScale = 1;
        public override ScriptableRenderPass GetPass() => new CreateRenderTargetPass(this);
    }

    public class CreateRenderTargetPass : SRPPass<CreateRenderTarget>
    {
        int[] colorIds;
        public CreateRenderTargetPass(CreateRenderTarget feature) : base(feature) { }

        public override bool CanExecute()
        {
            return base.CanExecute() 
                && Feature.colorTargetNames.Length>0 
                && !string.IsNullOrEmpty(Feature.depthTargetName);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if(colorIds == null || colorIds.Length != Feature.colorTargetNames.Length)
            {
                colorIds = new int[Feature.colorTargetNames.Length];
                RenderingTools.RenderTargetNameToInt(Feature.colorTargetNames, ref colorIds);
            }

            ref var cameraData = ref renderingData.cameraData;
            cmd.CreateTargets(cameraData.camera, colorIds, Feature.renderScale, Feature.isTargetHasDepthBuffer, Feature.isHDR);

            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                var depthId = Shader.PropertyToID(Feature.depthTargetName);
                cmd.CreateDepthTarget(cameraData.camera, depthId, Feature.renderScale);
            }
        }
    }
}
