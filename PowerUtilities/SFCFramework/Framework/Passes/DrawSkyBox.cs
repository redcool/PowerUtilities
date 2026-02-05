using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/DrawSkyBox")]
    public class DrawSkyBox : SRPFeature
    {
        [Header("URP Skybox")]
        [Tooltip("Reset urp SkyBox's target to current renderTarget[0]")]
        public bool isResetURPSkyBoxTarget;

        public override ScriptableRenderPass GetPass()
        {
            return new DrawSkyBoxPass(this);
        }
    }

    public class DrawSkyBoxPass : SRPPass<DrawSkyBox>
    {
        public DrawSkyBoxPass(DrawSkyBox feature) : base(feature)
        {
        }


        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;
            ref var cam = ref renderingData.cameraData.camera;

            RenderTargetHolder.GetLastTargets(renderer, out var colorTargets, out var depthTarget);

            //--------------- draw skybox
            cmd.BeginSampleExecute(Feature.GetName(), ref context);

            cmd.SetRenderTarget(colorTargets[0], depthTarget);
            cmd.Execute(ref context);

            context.DrawSkybox(cam);
            cmd.EndSampleExecute(Feature.GetName(), ref context);

            //--------------- update urp skybox target
            if (Feature.isResetURPSkyBoxTarget)
                SetupURPSkyboxTargets(renderer, cam, colorTargets[0], depthTarget);
        }

        public static void SetupURPSkyboxTargets(UniversalRenderer renderer, Camera cam,RTHandle colorTarget, RTHandle depthTarget)
        {
            var urpSkyPass = renderer.GetRenderPass<DrawSkyboxPass>(ScriptableRendererEx.PassFieldNames.m_DrawSkyboxPass);
            if(urpSkyPass != null)
                urpSkyPass.ConfigureTarget(colorTarget, depthTarget);
        }

        public static void SetupURPSkyboxTargets(UniversalRenderer renderer, Camera cam)
        {
            RenderTargetHolder.GetLastTargets(renderer, out var colorTargets, out var depthTarget);

            var urpSkyPass = renderer.GetRenderPass<DrawSkyboxPass>(ScriptableRendererEx.PassFieldNames.m_DrawSkyboxPass);
            urpSkyPass?.ConfigureTarget(colorTargets[0], depthTarget);
        }
    }
}
