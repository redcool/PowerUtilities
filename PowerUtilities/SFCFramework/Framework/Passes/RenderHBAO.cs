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

        [Tooltip("ao size")]
        [Range(0,1)] public float aoRangeMin=0.1f,aoRangeMax=1;

        [Tooltip("scale step distance(0.125)")]
        [Range(0.02f,.2f)] public float stepScale = 0.1f;

        [Tooltip("divide 360 into parts")]
        public int dirCount = 10;

        [Tooltip("move step alone a direction")]
        public int stepCount = 4;

        [Tooltip("calc normal from _CameraDepthTexture or use _CameraNormalsTexture")]
        public bool isNormalFromDepth = false;

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

            Feature.hbaoMat.SetFloat("_AORangeMin", Feature.aoRangeMin);
            Feature.hbaoMat.SetFloat("_AORangeMax", Feature.aoRangeMax);
            Feature.hbaoMat.SetFloat("_StepScale", Feature.stepScale);
            Feature.hbaoMat.SetInt("_DirCount", Feature.dirCount);
            Feature.hbaoMat.SetInt("_StepCount", Feature.stepCount);

            Feature.hbaoMat.SetKeyword("_NORMAL_FROM_DEPTH", Feature.isNormalFromDepth);

            cmd.BlitTriangle(BuiltinRenderTextureType.None, renderer.cameraColorTargetHandle, Feature.hbaoMat, 0);
            cmd.Execute(ref context);
        }
    }
}
