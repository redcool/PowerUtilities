using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName =SRP_FEATURE_PASSES_MENU+"/CopyColor")]
    public class CopyColor : SRPFeature
    {
        [Header("CopyColor Options")]
        public Material blitMat;
        public Material samplingMat;

        [Tooltip("diable urp asset 's opaque texture")]
        public bool disableURPOpaqueTexture = true;
        public Downsampling downSampling = Downsampling._2xBilinear;

        public override ScriptableRenderPass GetPass()
        {
            return new CopyColorPassWrapper(this);
        }
    }

    public class CopyColorPassWrapper : SRPPass<CopyColor>
    {
        CopyColorPass copyPass;
        public CopyColorPassWrapper(CopyColor feature) : base(feature)
        {
            copyPass = new CopyColorPass(renderPassEvent, Feature.samplingMat, Feature.blitMat);
            
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
            copyPass.Setup(ShaderPropertyIds._CameraColorAttachmentA,new RenderTargetHandle(ShaderPropertyIds._CameraOpaqueTexture), Feature.downSampling);

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
            copyPass.Execute(context, ref renderingData);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            copyPass.OnCameraCleanup(cmd);
        }
    }
}
