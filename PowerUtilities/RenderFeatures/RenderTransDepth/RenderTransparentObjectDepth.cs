namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    /// <summary>
    /// renderer transparent object to _CameraDepthTexture
    /// call DepthOnly pass
    /// </summary>
    public class RenderTransparentObjectDepth : ScriptableRendererFeature
    {
        class CustomRenderPass : ScriptableRenderPass
        {
            public Settings settings;

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var drawSettings = CreateDrawingSettings(new ShaderTagId("DepthOnly"), ref renderingData, SortingCriteria.CommonTransparent);

                FilteringSettings filterSettings = settings.filterSetting;

                var cmd = CommandBufferPool.Get();
                cmd.BeginSample(nameof(RenderTransparentObjectDepth));
                cmd.Execute(ref context);

                var renderer = (UniversalRenderer)renderingData.cameraData.renderer;
                var depthRH = renderer.GetCameraDepthTexture();
                cmd.SetRenderTarget(depthRH);
                cmd.Execute(ref context);

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

                cmd.SetRenderTarget(renderer.cameraColorTargetHandle);
                cmd.EndSample(nameof(RenderTransparentObjectDepth));
                cmd.Execute(ref context);

                CommandBufferPool.Release(cmd);
            }

        }

        CustomRenderPass m_ScriptablePass;

        [Serializable]
        public class Settings
        {
            public SimpleFilterSetting filterSetting = new();
        }
        public Settings settings;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new CustomRenderPass();
            m_ScriptablePass.settings = settings;

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }


}