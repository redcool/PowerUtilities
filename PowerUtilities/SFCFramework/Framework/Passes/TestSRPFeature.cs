using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/TestSRPFeature")]
    public class TestSRPFeature : SRPFeature
    {
        public LayerMask layers;

        public string featureName;
        public override ScriptableRenderPass GetPass()
        {
            return new TestSRPPass(this);
        }
    }


    public class TestSRPPass : SRPPass<TestSRPFeature>
    {
        public TestSRPPass(TestSRPFeature feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var desc = cameraData.cameraTargetDescriptor;

            var colorIds = new[] { 
                Shader.PropertyToID("_CameraColorAttachmentA"),
                Shader.PropertyToID("_ColorBuffer1"),
            };
            for (int i = 0; i < colorIds.Length; i++)
            {
                cmd.GetTemporaryRT(colorIds[i], desc.width, desc.height, 0,FilterMode.Bilinear,RenderTextureFormat.Default);
            }
            var depthBuffer = Shader.PropertyToID("depthBuffer");
            cmd.GetTemporaryRT(depthBuffer, desc.width, desc.height, 16, FilterMode.Point, RenderTextureFormat.Depth);

            var ids = colorIds.Select(id => new RenderTargetIdentifier(id)).ToArray();
            cmd.SetRenderTarget(ids, depthBuffer);
            cmd.ClearRenderTarget(true, true, Color.clear, 1);
            cmd.Execute(ref context);

            var shaderTags = new[] { 
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward")
            };
            var drawSettings = new DrawingSettings();
            for (int i = 0;i < shaderTags.Length;i++)
            {
                drawSettings.SetShaderPassName(i, shaderTags[i]);
            }
            var sortSettings = new SortingSettings(camera);
            sortSettings.criteria = SortingCriteria.RenderQueue;
            drawSettings.sortingSettings =sortSettings;
            drawSettings.perObjectData = renderingData.perObjectData;
            drawSettings.mainLightIndex = renderingData.lightData.mainLightIndex;

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            filterSettings.layerMask = Feature.layers;

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
            context.DrawSkybox(camera);

            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
        }
    }
}
