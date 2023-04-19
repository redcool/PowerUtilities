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
        [Range(0.1f,2)]public float renderScale = 1;

        public string[] colorTargetNames;
        public bool isTargetHasDepthBuffer;
        public bool isHDR;
        //public string depthTargetName;
        public override ScriptableRenderPass GetPass() => new CreateRenderTargetPass(this);
    }

    public class CreateRenderTargetPass : SRPPass<CreateRenderTarget>
    {
        int[] targetIds;
        public CreateRenderTargetPass(CreateRenderTarget feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if(targetIds == null || targetIds.Length != Feature.colorTargetNames.Length)
            {
                var targetIds = new int[Feature.colorTargetNames.Length];
                RenderingTools.RenderTargetNameToInt(Feature.colorTargetNames, ref targetIds);
            }

            ref var cameraData = ref renderingData.cameraData;
            cmd.CreateTargets(cameraData.camera, targetIds, Feature.renderScale, Feature.isTargetHasDepthBuffer, Feature.isHDR);

        }
    }
}
