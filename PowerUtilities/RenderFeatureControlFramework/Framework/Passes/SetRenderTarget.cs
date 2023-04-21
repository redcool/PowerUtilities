using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName =SRP_FEATURE_MENU+ "/SetRenderTarget")]
    public class SetRenderTarget : SRPFeature
    {
        public string[] colorTargetNames = new[] { nameof(ShaderPropertyIds._CameraColorAttachmentA) };

        [Tooltip("Will be _CameraDepthAttachment when empty")]
        public string depthTargetName = nameof(ShaderPropertyIds._CameraDepthAttachment);

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
            RenderTargetIdentifier depthId = cameraData.renderer.cameraDepthTarget;
            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                depthId = Shader.PropertyToID(Feature.depthTargetName);
            }

            if(SystemInfo.supportedRenderTargetCount < colorIds.Length)
                colorIds = colorIds.Take(SystemInfo.supportedRenderTargetCount).ToArray();

            cmd.SetRenderTarget(colorIds, depthId);

            if (Feature.clearTarget)
            {
                var cam = cameraData.camera;
                cmd.ClearRenderTarget(cam);
            }

        }
    }
}
