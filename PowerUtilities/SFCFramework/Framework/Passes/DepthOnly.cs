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
        RTHandle depthTextureHandle;
        public DepthOnlyPassWrapper(DepthOnly feature) : base(feature) {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            cmd.BeginSampleExecute(Feature.name, ref context);
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

            context.DrawRenderers(cmd,renderingData.cullResults, ref drawingSettings, ref filterSettings);

            cmd.EndSampleExecute(Feature.name, ref context);
        }

        private void SetupDeptexTarget(ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;
            depthTextureHandle = renderer.GetRTHandle(URPRTHandleNames.m_DepthTexture);
            var depthId = renderer.GetRenderTargetId(URPRTHandleNames.m_DepthTexture);

            var desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.Depth;
            desc.depthBufferBits = 24;
            desc.msaaSamples = 1;

#if UNITY_2022_1_OR_NEWER
#else
            if (depthId.IsNameIdEquals(0))
                cmd.GetTemporaryRT(ShaderPropertyIds._CameraDepthTexture, desc, FilterMode.Point);
#endif
            ConfigureTarget(depthTextureHandle);
            ConfigureClear(ClearFlag.Depth, Color.clear);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            SetupDeptexTarget(ref renderingData, cmd);
        }

    }
}
