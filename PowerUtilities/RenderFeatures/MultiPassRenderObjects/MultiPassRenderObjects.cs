using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_6000_0_OR_NEWER
using static UnityEngine.Rendering.Universal.RenderObjects;

#else
using CustomCameraSettings = UnityEngine.Experimental.Rendering.Universal.RenderObjects.CustomCameraSettings;
using FilterSettings = UnityEngine.Experimental.Rendering.Universal.RenderObjects.FilterSettings;
#endif

namespace UnityEngine.Experimental.Rendering.Universal
{


    public class MultiPassRenderObjects : ScriptableRendererFeature
    {
        [System.Serializable]
        public class RenderObjectsSettings
        {
            public string passTag = "MultiPassRenderObjects";
            public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;

            public FilterSettings filterSettings = new FilterSettings();

            public Material overrideMaterial = null;
            public int overrideMaterialPassIndex = 0;

            public bool overrideDepthState = false;
            public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
            public bool enableWrite = true;

            public StencilStateData stencilSettings = new StencilStateData();

            public CustomCameraSettings cameraSettings = new CustomCameraSettings();

            // MultiPass Options
            public int passCount = 11;
            public string passName = "FurPass";
        }

        public RenderObjectsSettings settings = new RenderObjectsSettings();

        //MultPassRenderObjectsPass renderObjectsPass;
        RenderObjectsPass renderObjectsPass;

        public override void Create()
        {
            FilterSettings filter = settings.filterSettings;

            // Render Objects pass doesn't support monos before rendering prepasses.
            // The camera is not setup before this point and all rendering is monoscopic.
            // Events before BeforeRenderingPrepasses should be used for input texture passes (shadow map, LUT, etc) that doesn't depend on the camera.
            // These monos are filtering in the UI, but we still should prevent users from changing it from code or
            // by changing the serialized data.
#if UNITY_2021_1_OR_NEWER
            var prepassId = RenderPassEvent.BeforeRenderingPrePasses;
#elif UNITY_2020
            var prepassId = RenderPassEvent.BeforeRenderingPrepasses;
#endif
            if (settings.Event < prepassId)
                settings.Event = prepassId;

            SetupFilterPassNames(ref filter);

            renderObjectsPass = new RenderObjectsPass(settings.passTag, settings.Event, filter.PassNames,
                filter.RenderQueueType, filter.LayerMask, settings.cameraSettings);

            renderObjectsPass.overrideMaterial = settings.overrideMaterial;
            renderObjectsPass.overrideMaterialPassIndex = settings.overrideMaterialPassIndex;

            if (settings.overrideDepthState)
#if UNITY_2023_1_OR_NEWER
                renderObjectsPass.SetDepthState(settings.enableWrite, settings.depthCompareFunction);
#else
                renderObjectsPass.SetDetphState(settings.enableWrite, settings.depthCompareFunction);
#endif

            if (settings.stencilSettings.overrideStencilState)
                renderObjectsPass.SetStencilState(settings.stencilSettings.stencilReference,
                    settings.stencilSettings.stencilCompareFunction, settings.stencilSettings.passOperation,
                    settings.stencilSettings.failOperation, settings.stencilSettings.zFailOperation);
        }

        private void SetupFilterPassNames(ref FilterSettings filter)
        {
            var passNames = new string[settings.passCount];
            for (int i = 0; i < passNames.Length; i++)
            {
                passNames[i] = settings.passName + i;
            }
            filter.PassNames = passNames;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(renderObjectsPass);
        }
    }
}

