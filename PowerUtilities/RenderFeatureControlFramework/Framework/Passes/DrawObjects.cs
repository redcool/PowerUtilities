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
using static UnityEditor.Experimental.GraphView.GraphView;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName =SRP_FEATURE_MENU+ "/DrawObjects")]
    public class DrawObjects : SRPFeature
    {
        [Header("Draw Objects Options")]
        public string[] shaderTags = new[] {
            "UniversalForwardOnly",
            "UniversalForward",
            "SRPDefaultUnlit"
        }; 
        
        public bool isOpaque = true;
        public LayerMask layers = -1;

        public StencilStateData stencilData;

        [Header("SkyBox")]
        public bool isDrawSkybox;
        public RenderPassEvent drawSkyboxEvent = RenderPassEvent.BeforeRenderingSkybox;

        public override ScriptableRenderPass GetPass() => new RenderObjectsPass(this);
    }

    public class RenderObjectsPass : SRPPass<DrawObjects>
    {
        DrawObjectsPass drawObjectsPass;
        DrawSkyboxPass drawSkyboxPass;
        public RenderObjectsPass(DrawObjects feature) : base(feature)
        {
            drawObjectsPass = GetDrawObjectsPass(feature);
            if (feature.isDrawSkybox)
                drawSkyboxPass = new DrawSkyboxPass(feature.drawSkyboxEvent);
        }

        public static DrawObjectsPass GetDrawObjectsPass(DrawObjects feature)
        {
            UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;

            var stencilData = feature.stencilData;
            var stencilState = StencilState.defaultValue;
            stencilState.enabled = stencilData.overrideStencilState;
            stencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            stencilState.SetFailOperation(stencilData.failOperation);
            stencilState.SetPassOperation(stencilData.passOperation);
            stencilState.SetZFailOperation(stencilData.zFailOperation);

            var shaderTagIds = feature.shaderTags
                .Select(name => new ShaderTagId(name))
                .ToArray();

            var renderQueueRange = feature.isOpaque ? RenderQueueRange.opaque : RenderQueueRange.transparent;

            return new DrawObjectsPass(feature.name, shaderTagIds, feature.isOpaque, feature.renderPassEvent, renderQueueRange, feature.layers, stencilState, stencilData.stencilReference);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            drawObjectsPass.OnCameraSetup(cmd, ref renderingData);

            if (drawSkyboxPass != null)
                drawSkyboxPass.OnCameraSetup(cmd, ref renderingData);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            drawObjectsPass.Execute(context, ref renderingData);

            if(drawSkyboxPass != null)
                drawSkyboxPass.Execute(context, ref renderingData);
        }
    }

}
