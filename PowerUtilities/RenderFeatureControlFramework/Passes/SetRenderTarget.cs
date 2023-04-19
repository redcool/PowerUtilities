using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static log4net.Appender.ColoredConsoleAppender;

namespace PowerUtilities.RenderFeatures
{
    public class SetRenderTarget : SRPFeature
    {
        public string[] colorTargetNames;
        public string depthTargetName;

        public bool clearTarget;
        public override ScriptableRenderPass GetPass() => new SetRenderTargetPass(this);
    }

    public class SetRenderTargetPass : SRPPass<SetRenderTarget>
    {
        RenderTargetIdentifier[] colorIds;
        int depthId;
        public SetRenderTargetPass(SetRenderTarget feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if(colorIds == null || colorIds.Length != Feature.colorTargetNames.Length)
            {
                RenderingTools.RenderTargetNameToIdentifier(Feature.colorTargetNames,ref colorIds);
            }
            ref var cameraData = ref renderingData.cameraData;

            depthId = Shader.PropertyToID(Feature.depthTargetName);

            cmd.SetRenderTarget(colorIds, depthId);

            if(Feature.clearTarget)
                cmd.ClearRenderTarget(cameraData.camera);
        }
    }
}
