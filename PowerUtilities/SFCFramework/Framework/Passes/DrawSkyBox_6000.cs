#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{

    public class DrawSkyBoxPass : SRPPass<DrawSkyBox>
    {
        RendererListHandle skyboxList;
        public DrawSkyBoxPass(DrawSkyBox feature) : base(feature)
        {
        }

        //public override bool IsTryRestoreLastTargets(Camera c) => c.IsGameCamera();

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);

            if (renderPassEvent <= RenderPassEvent.BeforeRenderingSkybox)
                renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

            skyboxList = defaultPassData.renderGraph.CreateSkyboxRendererList(renderingData.cameraData.camera);
            defaultPassData.rasterBuilder.UseRendererList(skyboxList);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;
            ref var cam = ref renderingData.cameraData.camera;

            //--------------- draw skybox
            cmd.BeginSampleExecute(Feature.GetName(), ref context);

            if (skyboxList.IsValid())
            {
                //context.GetRasterContext().cmd.SetupCameraProperties(cam);
                context.GetRasterContext().cmd.DrawRendererList(skyboxList);
            }

            cmd.EndSampleExecute(Feature.GetName(), ref context);
        }

    }
}
#endif
