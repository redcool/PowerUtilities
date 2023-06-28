using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEditor.Progress;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/CreateRenderTarget")]
    public class CreateRenderTarget : SRPFeature
    {
        [Header("Color Targets")]
        public List<RenderTargetInfo> colorTargetInfos = new List<RenderTargetInfo>();

        [Header("Depth Target")]
        public string depthTargetName;

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
                && !(Feature.colorTargetInfos.Count == 0 && string.IsNullOrEmpty(Feature.depthTargetName))
                ;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var renderScale = UniversalRenderPipeline.asset.renderScale;
            var samples = UniversalRenderPipeline.asset.msaaSampleCount;

            // override 
            if (Feature.overrideURPRenderScale)
                renderScale = Feature.renderScale;
            // for above SceneView 
            if (camera.cameraType > CameraType.Game)
                renderScale = 1;

            ref var cameraData = ref renderingData.cameraData;
            cmd.CreateTargets(cameraData.camera, Feature.colorTargetInfos, renderScale, samples);

            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                var depthId = Shader.PropertyToID(Feature.depthTargetName);
                cmd.CreateDepthTarget(cameraData.camera, depthId, renderScale, samples);
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            //var names = Enum.GetValues(typeof(RenderTextureFormat));
            //foreach (var item in names)
            //{
            //    var g = GraphicsFormatUtility.GetGraphicsFormat((RenderTextureFormat)item, false);
            //    Debug.Log(item+":"+g);
            //};
        }
    }
}
