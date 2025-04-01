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
        [Header("Skybox")]
        [Tooltip("reset urp SkyBox's target to current renderTarget")]
        public bool isResetTarget;

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
            if (Feature.isResetTarget)
                SetupSkyboxTargets(renderer, cam);
        }

        public static void SetupSkyboxTargets(UniversalRenderer renderer, Camera cam)
        {
            var urpSkyPass = renderer.GetRenderPass<DrawSkyboxPass>(ScriptableRendererEx.PassFieldNames.m_DrawSkyboxPass);

            var colorTarget = renderer.CameraColorTargetHandle();
            var depthTarget = renderer.CameraDepthTargetHandle();

            if (RenderTargetHolder.IsLastTargetValid())
            {
                colorTarget = RenderTargetHolder.LastColorTargetHandle;
                depthTarget = RenderTargetHolder.LastDepthTargetHandle;
            }
#if !UNITY_2021_1_OR_NEWER
            // restore CameraTarget ,below 2022
            if (c.IsSceneViewCamera())
            {
                var rth = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);
                //colorTarget = rth;
                depthTarget = rth;
            }
#endif
            urpSkyPass.ConfigureTarget(colorTarget, depthTarget);

        }
    }
}
