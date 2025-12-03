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

        [Tooltip("diable urp asset 's opaque texture")]
        public bool disableURPOpaqueTexture = true;
        public Downsampling downSampling = Downsampling._2xBilinear;

        public ClearFlag clearFlags;

        public override ScriptableRenderPass GetPass()
        {
            return new CopyColorPass(this);
        }
    }

    public class CopyColorPass : SRPPass<CopyColor>
    {
        /// <summary>
        /// only 1 rt
        /// </summary>
        public static RTHandle opaqueTextureHandle;
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
            if (!Feature.blitMat)
            {
                Feature.log = $"warning : blitMat is {Feature.blitMat}";
            }

            return base.CanExecute() && Feature.blitMat;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // like CopyColorPass.OnCameraSetup, canot call it directly.
            ref var cameraData = ref renderingData.cameraData;
            var renderer = cameraData.renderer as UniversalRenderer;

            var desc = cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;
            desc.depthBufferBits = 0;
            desc.colorFormat = RenderTextureFormat.Default;
            if (Feature.downSampling == Downsampling._2xBilinear)
            {
                desc.width >>= 1;
                desc.height >>= 1;
            }
            else if (Feature.downSampling == Downsampling._4xBox || Feature.downSampling == Downsampling._4xBilinear)
            {
                desc.width >>= 2;
                desc.height >>= 2;
            }
            var filterMode = Feature.downSampling == Downsampling.None ? FilterMode.Point : FilterMode.Bilinear;
            //cmd.GetTemporaryRT(ShaderPropertyIds._CameraOpaqueTexture, desc, Feature.downSampling == Downsampling.None ? FilterMode.Point : FilterMode.Bilinear);
            RenderingUtils.ReAllocateIfNeeded(ref opaqueTextureHandle, desc, name: nameof(ShaderPropertyIds._CameraOpaqueTexture), filterMode: filterMode);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            var renderer = renderingData.cameraData.renderer as UniversalRenderer;

            var srcHandle = renderer.GetCameraColorAttachmentA();

            cmd.BlitTriangle(srcHandle, 
                opaqueTextureHandle, 
                Feature.blitMat, 
                0,
                isTryReplaceUrpTarget: false,
                clearFlags: Feature.clearFlags
                );
            cmd.SetGlobalTexture(ShaderPropertyIds._CameraOpaqueTexture, opaqueTextureHandle);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {

        }
    }
}
