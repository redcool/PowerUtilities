using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.Features
{
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public Material blitMat;
            public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
            public int passEventOffset = 10;
            public LayerMask layerMask;

            public StencilStateData stencilStateData;

        }
        public Settings settings;

        RenderUIPass uiPass;

        /// <inheritdoc/>
        public override void Create()
        {

        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            if ((cameraData.camera.cullingMask & settings.layerMask) == 0)
                return;

            if (uiPass == null)
            {
                var stencilState = new StencilState();
                stencilState.SetCompareFunction(settings.stencilStateData.stencilCompareFunction);
                stencilState.SetFailOperation(settings.stencilStateData.failOperation);
                stencilState.SetPassOperation(settings.stencilStateData.passOperation);
                stencilState.SetZFailOperation(settings.stencilStateData.zFailOperation);
                stencilState.enabled = settings.stencilStateData.overrideStencilState;

                uiPass = new RenderUIPass(renderer.cameraColorTarget, settings.blitMat,
                    settings.layerMask, stencilState, settings.stencilStateData.stencilReference);
            }
            uiPass.renderPassEvent =settings.passEvent+settings.passEventOffset;

            renderer.EnqueuePass(uiPass);
        }

    }


}