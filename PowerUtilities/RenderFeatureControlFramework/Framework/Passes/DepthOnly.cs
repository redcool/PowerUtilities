using PowerUtilities.RenderFeatures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/DepthOnly")]
    public class DepthOnly : SRPFeature
    {
        [Header("Depth Only")]
        public LayerMask layerMask = -1;
        public override ScriptableRenderPass GetPass() => new DepthOnlyPassWrapper(this);
    }

    public class DepthOnlyPassWrapper : SRPPass<DepthOnly>
    {
        DepthOnlyPass depthOnlyPass;
        public DepthOnlyPassWrapper(DepthOnly feature) : base(feature) {
            depthOnlyPass = new DepthOnlyPass(feature.renderPassEvent, RenderQueueRange.opaque, feature.layerMask);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            depthOnlyPass.Setup(cameraData.cameraTargetDescriptor, new RenderTargetHandle(ShaderPropertyIds._CameraDepthTexture));
        }
        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            cmd.BeginSampleExecute(Feature.name, ref context);

            depthOnlyPass.Execute(context, ref renderingData);

            cmd.EndSampleExecute(Feature.name, ref context);
        }
    }
}
