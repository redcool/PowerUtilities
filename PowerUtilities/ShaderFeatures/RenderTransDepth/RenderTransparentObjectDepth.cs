namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class RenderTransparentObjectDepth : ScriptableRendererFeature
    {
        class CustomRenderPass : ScriptableRenderPass
        {
            public Settings settings;

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var drawSettings = CreateDrawingSettings(new ShaderTagId("DepthOnly"), ref renderingData, SortingCriteria.CommonTransparent);

                var filterSettings = new FilteringSettings(RenderQueueRange.transparent);
                filterSettings.layerMask = settings.layerMask;

                var cmd = CommandBufferPool.Get();
                cmd.BeginSample(nameof(RenderTransparentObjectDepth));
                Execute(context, cmd);

                cmd.SetRenderTarget("_CameraDepthTexture");
                Execute(context, cmd);

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

                cmd.SetRenderTarget("_CameraColorAttachmentA");
                cmd.EndSample(nameof(RenderTransparentObjectDepth));
                Execute(context, cmd);

                CommandBufferPool.Release(cmd);

                static void Execute(ScriptableRenderContext context, CommandBuffer cmd)
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
            }

        }

        CustomRenderPass m_ScriptablePass;

        [Serializable]
        public class Settings
        {
            public LayerMask layerMask = -1;
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