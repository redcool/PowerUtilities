using PowerUtilities.RenderFeatures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities
{
    [Tooltip("Fill _CamerDepthTexture render scene once")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/DepthOnly")]
    public class DepthOnly : SRPFeature
    {
        [Header("Depth Only")]
        public LayerMask layerMask = -1;
        public override ScriptableRenderPass GetPass() => new DepthOnlyPassWrapper(this);
    }

    public class DepthOnlyPassWrapper : SRPPass<DepthOnly>
    {
        public DepthOnlyPassWrapper(DepthOnly feature) : base(feature) {
        }


        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            cmd.BeginSampleExecute(Feature.name, ref context);

            SetupDeptexTarget(ref context, ref renderingData, cmd);

            var camera = renderingData.cameraData.camera;
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, Feature.layerMask);

            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            var drawingSettings = new DrawingSettings(new ShaderTagId("DepthOnly"), sortingSettings)
            {
                enableInstancing = true,
                enableDynamicBatching = renderingData.supportsDynamicBatching,
                mainLightIndex = renderingData.lightData.mainLightIndex,
                perObjectData = PerObjectData.None,
            };

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filterSettings);

            cmd.EndSampleExecute(Feature.name, ref context);
        }

        private void SetupDeptexTarget(ref ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.Depth;
            desc.depthBufferBits = 32;
            desc.msaaSamples = 1;

            cmd.GetTemporaryRT(ShaderPropertyIds._CameraDepthTexture, desc, FilterMode.Point);
            cmd.SetRenderTarget(ShaderPropertyIds._CameraDepthTexture);
            cmd.ClearRenderTarget(true, false, Color.clear);

            cmd.Execute(ref context);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(ShaderPropertyIds._CameraDepthTexture);
        }
    }
}
