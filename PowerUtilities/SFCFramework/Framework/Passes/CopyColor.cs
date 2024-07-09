using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

#if UNITY_2020_3
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities.RenderFeatures
{
    [Tooltip("Copy _CameraColorAttachmentA to _CameraOpaqueTexture, can control execution order ")]
    [CreateAssetMenu(menuName =SRP_FEATURE_PASSES_MENU+"/CopyColor")]
    public class CopyColor : SRPFeature
    {
        [Header("CopyColor Options")]

        [LoadAsset("CopyColor.mat")]
        public Material blitMat;

        [LoadAsset("Sampling Mat.mat")]
        public Material samplingMat;

        [Tooltip("diable urp asset 's opaque texture")]
        public bool disableURPOpaqueTexture = true;
        public Downsampling downSampling = Downsampling._2xBilinear;

        public override ScriptableRenderPass GetPass()
        {
            return new CopyColorPass(this);
        }
    }

    public class CopyColorPass : SRPPass<CopyColor>
    {
        //CopyColorPass copyPass;
        public CopyColorPass(CopyColor feature) : base(feature)
        {
            var asset = UniversalRenderPipeline.asset;
            if(Feature.disableURPOpaqueTexture)
                asset.supportsCameraOpaqueTexture = false;
        }

        public override bool CanExecute()
        {
            Feature.log = "";
            if (!Feature.blitMat || !Feature.samplingMat)
            {
                Feature.log = $"warning : blitMat is {Feature.blitMat}, samplingMat is {Feature.samplingMat}";
            }

            return base.CanExecute()
                && Feature.blitMat
                && Feature.samplingMat
                ;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // like CopyColorPass.OnCameraSetup, canot call it directly.
            ref var cameraData = ref renderingData.cameraData;
            var desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.colorFormat = RenderTextureFormat.Default;
            if (Feature.downSampling == Downsampling._2xBilinear)
            {
                desc.width >>=1;
                desc.height >>=1;
            }
            else if (Feature.downSampling == Downsampling._4xBox || Feature.downSampling == Downsampling._4xBilinear)
            {
                desc.width >>= 2;
                desc.height >>= 2;
            }

            cmd.GetTemporaryRT(ShaderPropertyIds._CameraOpaqueTexture, desc, Feature.downSampling == Downsampling.None ? FilterMode.Point : FilterMode.Bilinear);

        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var renderer = renderingData.cameraData.renderer as UniversalRenderer;

            var srcHandle = renderer.GetCameraColorAttachmentA();

            cmd.BlitTriangle(srcHandle, ShaderPropertyIds._CameraOpaqueTexture, Feature.blitMat, 0, isTryReplaceUrpTarget: false);
            cmd.SetGlobalTexture(ShaderPropertyIds._CameraOpaqueTexture, ShaderPropertyIds._CameraOpaqueTexture);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }
    }
}
