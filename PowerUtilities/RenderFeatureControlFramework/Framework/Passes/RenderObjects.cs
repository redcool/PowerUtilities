using PowerUtilities.RenderFeatures;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Experimental.Rendering.Universal.RenderObjects;

namespace PowerUtilities
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/RenderObjects")]
    public class RenderObjects : SRPFeature
    {
        public FilterSettings filterSettings = new FilterSettings();

        public Material overrideMaterial = null;
        public int overrideMaterialPassIndex = 0;

        [EditorGroup("Override Depth",true)]
        public bool overrideDepthState = false;
        [EditorGroup("Override Depth")] public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
        [EditorGroup("Override Depth")] public bool enableWrite = true;

        public StencilStateData stencilSettings = new StencilStateData();

        public CustomCameraSettings cameraSettings = new CustomCameraSettings();

        public override ScriptableRenderPass GetPass() => new RenderObjectsWrapper(this);
    }

    public class RenderObjectsWrapper : SRPPass<RenderObjects> {
        RenderObjectsPass renderObjectPass;
        public RenderObjectsWrapper(RenderObjects feature) : base(feature)
        {
            var filter = Feature.filterSettings;
            renderObjectPass = new RenderObjectsPass(Feature.name, Feature.renderPassEvent+Feature.renderPassEventOffset
                , filter.PassNames, filter.RenderQueueType, filter.LayerMask, Feature.cameraSettings);
            renderObjectPass.overrideMaterial = Feature.overrideMaterial;
            renderObjectPass.overrideMaterialPassIndex = Feature.overrideMaterialPassIndex;

            if(Feature.overrideDepthState) 
                renderObjectPass.SetDetphState(Feature.overrideDepthState);

            var stencil = Feature.stencilSettings;
            if (Feature.stencilSettings.overrideStencilState)
                renderObjectPass.SetStencilState(stencil.stencilReference, stencil.stencilCompareFunction, stencil.passOperation,
                    stencil.failOperation, stencil.zFailOperation);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            renderObjectPass.Execute(context, ref renderingData);
        }

        
    }
}
