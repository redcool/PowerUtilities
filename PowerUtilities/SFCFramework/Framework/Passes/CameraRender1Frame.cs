using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif
namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// SetTarget ,DrawScene
    /// </summary>
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/CameraRender1Frame")]
    public class CameraRender1Frame : SRPFeature
    {
        public LayerMask layers = -1;

        [Tooltip("set enabled false when render 1 frame")]
        public bool isDisableWhenDone = true;

        public bool isUseOverrideMat;
        [LoadAsset("SFC_ShowOverdrawAdd.mat")]
        public Material overrideMat;

        public RenderTexture[] colorTargets;
        public RenderTexture depthTarget;

        public string[] shaderTags = new[]
        {
            "SRPDefaultUnlit"
            ,"UniversalForward"
        };
        [Tooltip("viewport uv rect")]
        public Vector4 viewportUVRect = new Vector4(0,0,1,1);

        public override ScriptableRenderPass GetPass()
        {
            return new CameraRender1FramePass(this);
        }
    }


    public class CameraRender1FramePass : SRPPass<CameraRender1Frame>
    {
        public CameraRender1FramePass(CameraRender1Frame feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            cmd.BeginSampleExecute(nameof(CameraRender1FramePass),ref context);

            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;
            ref var cameraData = ref renderingData.cameraData;
            var desc = cameraData.cameraTargetDescriptor;
            var cam = cameraData.camera;

            var colorIds = Feature.colorTargets
                .Where( target => target)
                .Select(target => new RenderTargetIdentifier(target))
                .ToArray();

            if (colorIds.Length <= 0 || Feature.depthTarget == default)
                return;
            var colorRT0 = Feature.colorTargets[0];

            cmd.SetRenderTarget(colorIds, Feature.depthTarget);
            // set clear
            var isClearColor = (cam.clearFlags & CameraClearFlags.Color) > 0;
            var isClearDepth = (cam.clearFlags & CameraClearFlags.Depth) > 0;
            cmd.ClearRenderTarget(isClearDepth, isClearColor, cam.backgroundColor);
            // Set viewport
            cmd.SetViewport(new Rect(
                Mathf.Clamp01(Feature.viewportUVRect.x) * colorRT0.width,
                Mathf.Clamp01(Feature.viewportUVRect.y) * colorRT0.height,
                Mathf.Clamp01(Feature.viewportUVRect.z) * colorRT0.width,
                Mathf.Clamp01(Feature.viewportUVRect.w) * colorRT0.height
                ));

            cmd.Execute(ref context);


            var shaderTags = Feature.shaderTags
                .Where(tag => !string.IsNullOrEmpty(tag))
                .Select(tag => new ShaderTagId(tag))
                .ToArray();

            var drawSettings = new DrawingSettings();
            for (int i = 0;i < shaderTags.Length;i++)
            {
                drawSettings.SetShaderPassName(i, shaderTags[i]);
            }
            var sortSettings = new SortingSettings(camera);
            sortSettings.criteria = SortingCriteria.CommonOpaque;

            drawSettings.sortingSettings =sortSettings;
            drawSettings.perObjectData = renderingData.perObjectData;
            drawSettings.mainLightIndex = renderingData.lightData.mainLightIndex;

            if(Feature.isUseOverrideMat)
                drawSettings.overrideMaterial = Feature.overrideMat;

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            filterSettings.layerMask = Feature.layers;

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
            context.DrawSkybox(camera);

            sortSettings.criteria = SortingCriteria.CommonTransparent;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

            cmd.EndSampleExecute(nameof(CameraRender1FramePass), ref context);

            if(Feature.isDisableWhenDone)
                Feature.enabled = false;
        }
    }
}
