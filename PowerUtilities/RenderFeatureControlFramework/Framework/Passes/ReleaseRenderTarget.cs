using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/ReleaseRenderTarget")]
    public class ReleaseRenderTarget : SRPFeature
    {
        [Header("Targets")]
        public string[] targetNames;
        public override ScriptableRenderPass GetPass() => new ReleaseRenderTargetPass(this);
    }

    public class ReleaseRenderTargetPass : SRPPass<ReleaseRenderTarget>
    {
        int[] targetIds;
        public ReleaseRenderTargetPass(ReleaseRenderTarget feature) : base(feature) { }

        public override bool CanExecute()
        {
            return base.CanExecute() 
                && !(Feature.targetNames.Length == 0)
                ;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            //if (targetIds == null || targetIds.Length != Feature.targetNames.Length)
            {
                targetIds = new int[Feature.targetNames.Length];
                RenderingTools.RenderTargetNameToInt(Feature.targetNames, ref targetIds);
            }

            targetIds.ForEach(id => cmd.ReleaseTemporaryRT(id));
            
        }
    }
}
