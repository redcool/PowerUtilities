using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName =SRP_FEATURE_PASSES_MENU+ "/SetRenderTarget")]
    public class SetRenderTarget : SRPFeature
    {
        [Tooltip("use CurrentActive when item empty")]
        public string[] colorTargetNames = new[] { nameof(ShaderPropertyIds._CameraColorAttachmentA) };

        [Tooltip("use CameraTarget(or renderer.cameraDepthTarget) when empty")]
        public string depthTargetName;

        public bool clearTarget;
        public override ScriptableRenderPass GetPass() => new SetRenderTargetPass(this);
    }

    public class SetRenderTargetPass : SRPPass<SetRenderTarget>
    {
        RenderTargetIdentifier[] colorIds;

        public SetRenderTargetPass(SetRenderTarget feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            //Feature.colorTargetNames = Feature.colorTargetNames
            //    .Where(item => !string.IsNullOrEmpty(item))
            //    .ToArray();

            return base.CanExecute() 
                && Feature.colorTargetNames.Length > 0;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (colorIds == null || colorIds.Length != Feature.colorTargetNames.Length)
            {
                RenderingTools.RenderTargetNameToIdentifier(Feature.colorTargetNames, ref colorIds);
            }
            ref var cameraData = ref renderingData.cameraData;

            // set depth target id
            RenderTargetIdentifier depthId = UniversalRenderPipeline.asset.supportsCameraDepthTexture ? cameraData.renderer.cameraDepthTarget : BuiltinRenderTextureType.CameraTarget;
            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                depthId = Shader.PropertyToID(Feature.depthTargetName);
            }

            if(SystemInfo.supportedRenderTargetCount < colorIds.Length)
                colorIds = colorIds.Take(SystemInfo.supportedRenderTargetCount).ToArray();

            //cmd.SetRenderTarget(colorIds, depthId);
            ConfigureTarget(colorIds, depthId);

            if (Feature.clearTarget)
            {
                var cam = cameraData.camera;
                cmd.ClearRenderTarget(cam);
            }

        }
    }
}
