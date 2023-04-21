using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName =SRP_FEATURE_MENU+ "/DrawObjects")]
    public class DrawObjects : SRPFeature
    {
        [Header("Draw Options")]
        public string profilerTag = "DrawObjects";
        public string[] shaderTags = new[] {
            "UniversalForwardOnly",
            "UniversalForward",
            "SRPDefaultUnlit"
        }; 
        
        public bool isOpaque;
        public LayerMask layers = -1;

        public StencilStateData stencilData;

        public override ScriptableRenderPass GetPass()
        {
            UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;

            var stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            var shaderTagIds = shaderTags
                .Select(name => new ShaderTagId(name))
                .ToArray();

            var renderQueueRange = isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;

            return new DrawObjectsPass(profilerTag,shaderTagIds,isOpaque,renderPassEvent,renderQueueRange,layers,stencilState,stencilData.stencilReference);
        }
    }

}
