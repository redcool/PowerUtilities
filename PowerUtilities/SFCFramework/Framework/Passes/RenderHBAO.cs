using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    public class RenderHBAO : SRPFeature
    {
        [Header(nameof(RenderHBAO))]
        [LoadAsset("Hidden_Unlit_HBAO.mat")]
        public Material hbaoMat;
        public override ScriptableRenderPass GetPass()
            => new RenderHBAOPass(this);
    }

    public class RenderHBAOPass : SRPPass<RenderHBAO>
    {
        public RenderHBAOPass(RenderHBAO feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var cam = renderingData.cameraData.camera;
            var renderer = renderingData.cameraData.renderer;

            var invView = cam.cameraToWorldMatrix;
            var invProj = GL.GetGPUProjectionMatrix(cam.projectionMatrix,false).inverse;
            var invVP = (invView * invProj);

            Feature.hbaoMat.SetMatrix("invVP", invVP);
            cmd.BlitTriangle(BuiltinRenderTextureType.None, renderer.cameraColorTargetHandle, Feature.hbaoMat, 0);
            cmd.Execute(ref context);
        }
    }
}
