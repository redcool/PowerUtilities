using PowerUtilities.RenderFeatures;
using System;
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

        public DepthSettings depthOverrideSettings = new DepthSettings();
        public StencilStateData stencilOverrideSettings = new StencilStateData();
        public CustomCameraSettings cameraOverrideSettings = new CustomCameraSettings();

        public override ScriptableRenderPass GetPass() => new RenderObjectsWrapper(this);
    }

    [Serializable]
    public class DepthSettings
    {
        public bool overrideDepthState = false;
        public CompareFunction depthCompareFunction = CompareFunction.LessEqual;
        public bool enableWrite = true;

    }

    public class RenderObjectsWrapper : SRPPass<RenderObjects> {
        RenderObjectsPass renderObjectPass;
        public RenderObjectsWrapper(RenderObjects feature) : base(feature)
        {
            var filter = Feature.filterSettings;
            renderObjectPass = new RenderObjectsPass(Feature.name, Feature.renderPassEvent+Feature.renderPassEventOffset
                , filter.PassNames, filter.RenderQueueType, filter.LayerMask, Feature.cameraOverrideSettings);
            renderObjectPass.overrideMaterial = Feature.overrideMaterial;
            renderObjectPass.overrideMaterialPassIndex = Feature.overrideMaterialPassIndex;

            var depth = Feature.depthOverrideSettings;
            if(depth.overrideDepthState) 
                renderObjectPass.SetDetphState(depth.overrideDepthState);

            var stencil = Feature.stencilOverrideSettings;
            if (Feature.stencilOverrideSettings.overrideStencilState)
                renderObjectPass.SetStencilState(stencil.stencilReference, stencil.stencilCompareFunction, stencil.passOperation,
                    stencil.failOperation, stencil.zFailOperation);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            renderObjectPass.Execute(context, ref renderingData);
        }

        
    }
}
