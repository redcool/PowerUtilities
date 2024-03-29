using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [Tooltip("Create RenderTargets (Color or depth)")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/CreateRenderTarget")]
    public class CreateRenderTarget : SRPFeature
    {
        [Header("Targets")]
        public List<RenderTargetInfo> colorTargetInfos = new List<RenderTargetInfo>();

        [Header("Render Scale")]
        public bool overrideURPRenderScale=false;
        [Range(0.1f,2)]public float renderScale = 1;
        public override ScriptableRenderPass GetPass() => new CreateRenderTargetPass(this);
    }

    public class CreateRenderTargetPass : SRPPass<CreateRenderTarget>
    {

        public CreateRenderTargetPass(CreateRenderTarget feature) : base(feature) { }

        public override bool CanExecute()
        {
            return base.CanExecute() 
                && Feature.colorTargetInfos.Count != 0
                && !camera.IsReflectionCamera()
                ;

        }

        public void CreateTargets(CommandBuffer cmd)
        {
            var renderScale = UniversalRenderPipeline.asset.renderScale;
            var samples = UniversalRenderPipeline.asset.msaaSampleCount;

            // override 
            if (Feature.overrideURPRenderScale)
                renderScale = Feature.renderScale;

            // for above SceneView 
            if (camera.cameraType > CameraType.Game)
                renderScale = 1;

            cmd.CreateTargets(camera, Feature.colorTargetInfos, renderScale, samples);
        }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //CreateTargets(cmd);
        }
        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            CreateTargets(cmd);
        }
    }
}
