using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PowerUtilities;
using System.Numerics;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Draw light geometry one by one
    /// 
    /// dir : plane
    /// spot : cone
    /// point : sphere
    /// other : use texture
    /// </summary>
    [Tooltip("Defered Lighting")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/DeferedLighting")]
    public class DeferredLighting : SRPFeature
    {


        /// <summary>
        /// 
        ///
        /// sv_target0 , xyz : albedo+giColor, w: emission.z
        /// sv_target1, xy:normal.xy,zw:emission.xy
        /// sv_target2, xy(16) : motion vector.xy
        /// sv_target3 , xyz:pbrMask, w:mainLightShadow
        /// 
        /// </summary>
            [Header("Objects")]
        public string _GBuffer0 = nameof(_GBuffer0);
        public string _GBuffer1 = nameof(_GBuffer1);
        public string _GBuffer2 = nameof(_GBuffer2);
        public string _MotionVectorTexture = nameof(_MotionVectorTexture);

        public string deferedTag = "UniversalGBuffer";
        public int layers = -1;

        //[Header("Output")]
        //public string targetName;
        public override ScriptableRenderPass GetPass() => new DeferedLightingPass(this);
    }

    public class DeferedLightingPass : SRPPass<DeferredLighting>
    {
        public DeferedLightingPass(DeferredLighting feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() 
                ;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraDate = ref renderingData.cameraData;

            SetupTargets(ref context, ref renderingData, cmd);
            DrawScene(ref context, ref renderingData, cmd, cameraDate);
        }

        private void DrawScene(ref ScriptableRenderContext context,ref RenderingData renderingData, CommandBuffer cmd, CameraData cameraDate)
        {
            var sortingSettings = new SortingSettings(cameraDate.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };

            var drawSettings = new DrawingSettings(new ShaderTagId(Feature.deferedTag), sortingSettings)
            {
                enableDynamicBatching = true,
                perObjectData = PerObjectData.LightProbe | PerObjectData.ReflectionProbes | PerObjectData.Lightmaps | PerObjectData.LightData
                | PerObjectData.MotionVectors | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask,
            };

            //drawSettings.SetShaderPassNames(RenderingTools.urpForwardShaderPassNames);

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, Feature.layers);

            context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings);
        }

        private void SetupTargets(ref ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            int gbuffer0 = Shader.PropertyToID(Feature._GBuffer0);
            int gbuffer1 = Shader.PropertyToID(Feature._GBuffer1);
            int gbuffer2 = Shader.PropertyToID(Feature._GBuffer2);
            int gbuffer3 = Shader.PropertyToID(Feature._MotionVectorTexture);

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.msaaSamples = 1;

            var motionDesc = desc;
            motionDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16_SFloat;

            cmd.GetTemporaryRT(gbuffer0, desc);
            cmd.GetTemporaryRT(gbuffer1, desc);
            cmd.GetTemporaryRT(gbuffer2, desc);
            cmd.GetTemporaryRT(gbuffer3, motionDesc);

            var depthDesc = desc;
            depthDesc.colorFormat = RenderTextureFormat.Depth;
            depthDesc.depthBufferBits = 24;
            cmd.GetTemporaryRT(ShaderPropertyIds._CameraDepthAttachment, depthDesc);

            var colorIds = new RenderTargetIdentifier[] { gbuffer0, gbuffer1, gbuffer2 , gbuffer3 };
            var depthId = ShaderPropertyIds._CameraDepthAttachment;

            cmd.SetRenderTarget(colorIds, depthId);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.Execute(ref context);
        }
    }
}
