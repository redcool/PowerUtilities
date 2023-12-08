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
        public string depthTextureName = "_CameraDepthTexture";
        public override ScriptableRenderPass GetPass() => new DepthOnlyPassWrapper(this);
    }

    public class DepthOnlyPassWrapper : SRPPass<DepthOnly>
    {
        RenderTexture depthTex;
        public DepthOnlyPassWrapper(DepthOnly feature) : base(feature) {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && !string.IsNullOrEmpty(Feature.depthTextureName);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var depthTexId = Shader.PropertyToID(Feature.depthTextureName);

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

            SetupDeptexTarget(ref renderingData, cmd,depthTexId);

            cmd.SetRenderTarget(depthTex);
            cmd.ClearRenderTarget(true, false, Color.clear);
            cmd.Execute(ref context);

            context.DrawRenderers(cmd,renderingData.cullResults, ref drawingSettings, ref filterSettings);

            cmd.EndSampleExecute(Feature.name, ref context);
        }

        private void SetupDeptexTarget(ref RenderingData renderingData, CommandBuffer cmd,int depthTexId)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;

            var desc = cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.Depth;
            desc.depthBufferBits = 24;
            desc.msaaSamples = 1;
            //cmd.GetTemporaryRT(depthTexId, desc);

            if(!depthTex || depthTex.width != desc.width || depthTex.height != desc.height)
            {
                if(depthTex)
                    depthTex.Release();

                depthTex = new RenderTexture(desc);
            }
            cmd.SetGlobalTexture(depthTexId, depthTex);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            if (depthTex)
                depthTex.Release();
        }

    }
}
