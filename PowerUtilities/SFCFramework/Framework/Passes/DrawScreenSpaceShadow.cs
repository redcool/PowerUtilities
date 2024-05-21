using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/DrawScreenSpaceShadow")]
    public class DrawScreenSpaceShadow : SRPFeature
    {
        [LoadAsset("Utils_DrawScreenSpaceShadow.mat")]
        public Material screenShadowMat;
        public override ScriptableRenderPass GetPass()  => new DrawScreenSpaceShadowPass(this);

    }

    public class DrawScreenSpaceShadowPass : SRPPass<DrawScreenSpaceShadow>
    {
        public override bool CanExecute()
        {
            return base.CanExecute() && Feature.screenShadowMat;
        }
        public DrawScreenSpaceShadowPass(DrawScreenSpaceShadow feature) : base(feature)
        {
        }

        Matrix4x4[] mainLightShadows = new Matrix4x4[4];
        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var colorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;

            cmd.EnableShaderKeyword("_MAIN_LIGHT_SHADOWS_SCREEN");
            cmd.BlitTriangle(BuiltinRenderTextureType.None, colorTarget.nameID, Feature.screenShadowMat, 0);
        }
    }
}
