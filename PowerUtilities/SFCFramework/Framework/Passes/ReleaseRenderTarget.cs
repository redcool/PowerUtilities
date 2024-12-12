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
        [Tooltip("run this pass after all camera rendering")]
        public bool isRunAfterCameraStackRendering;

        [Header("Targets")]
        public string[] targetNames;

        [Header("URP RTs")]
        [Tooltip("enable removeURPRT, if dont clear, dont check it")]
        public bool IsRemoveURPRT;

        [Tooltip("urp rt names")]
        public URPRTHandleNames[] urpRTNames;


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
            //===== release rtid
            //if (targetIds == null || targetIds.Length != Feature.targetNames.Length)
            {
                targetIds = new int[Feature.targetNames.Length];
                RenderingTools.RenderTargetNameToInt(Feature.targetNames, ref targetIds);
            }

            foreach (int id in targetIds)
            {
                cmd.ReleaseTemporaryRT(id);
            }

            //========= release renderTexture
            foreach (var rtName in Feature.targetNames)
            {
                RenderTextureTools.DestroyRT(rtName);
            }

            //========= release urp rtHandles
            if (Feature.IsRemoveURPRT && Feature.urpRTNames != null)
                foreach (var item in Feature.urpRTNames)
                {
                    var renderer = UniversalRenderPipeline.asset.GetDefaultRenderer();
                    var rth = renderer.GetRTHandle(item, true);
                    rth?.Release();
                }
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
