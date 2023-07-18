using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [Tooltip("Release targets")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/ReleaseRenderTarget")]
    public class ReleaseRenderTarget : SRPFeature
    {
        [Header("Targets")]
        public string[] targetNames;

        [Tooltip("run this pass after all camera rendering")]
        public bool isRunAfterCameraStackRendering;
        public override ScriptableRenderPass GetPass() => new ReleaseRenderTargetPass(this);
    }

    public class ReleaseRenderTargetPass : SRPPass<ReleaseRenderTarget>
    {
        int[] targetIds;
        public ReleaseRenderTargetPass(ReleaseRenderTarget feature) : base(feature) { }

        public override bool CanExecute()
        {
            if (Feature.targetNames == null || Feature.targetNames.Length == 0)
                return false;

            if (Feature.isRunAfterCameraStackRendering)
                return true;

            return base.CanExecute();
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            
        }

        public void ReleaseTargets(CommandBuffer cmd)
        {
            //if (targetIds == null || targetIds.Length != Feature.targetNames.Length)
            {
                targetIds = new int[Feature.targetNames.Length];
                RenderingTools.RenderTargetNameToInt(Feature.targetNames, ref targetIds);
            }

            targetIds.ForEach(id => cmd.ReleaseTemporaryRT(id));
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (!Feature.isRunAfterCameraStackRendering)
                ReleaseTargets(cmd);
        }
        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            if (Feature.isRunAfterCameraStackRendering)
                ReleaseTargets(cmd);
        }

    }
}
