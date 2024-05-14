using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PowerUtilities;
using System.Linq;
using Unity.Mathematics;

#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Draw light geometry one by one
    /// 
    /// dir : plane
    /// spot : cone
    /// point : sphere
    /// other : use texture
    /// 
    /// 
    /// * GBuffer, check PowerLit's GBuffer pass
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
        [Header("Lights")]
        public Material lightMat;

        //[Header("Output")]
        //public string targetName;
        public override ScriptableRenderPass GetPass() => new DeferedLightingPass(this);
    }

    public class DeferedLightingPass : SRPPass<DeferredLighting>
    {

        RenderTargetIdentifier[] colorTargets = new RenderTargetIdentifier[4];

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
            var renderer = (UniversalRenderer)cameraDate.renderer;
            var colorAttachmentA = renderer.GetCameraColorAttachmentA();

            SetupTargets(ref context, ref renderingData, cmd);
            DrawScene(ref context, ref renderingData, cmd, cameraDate);

            if (!Feature.lightMat)
                return;

            cmd.SetRenderTarget(colorAttachmentA);
            cmd.Execute(ref context);
#if UNITY_2021_3_OR_NEWER
            var lightGroups = Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
#else
            var lightGroups = Object.FindObjectsOfType<Light>()
#endif
            .GroupBy(light => light.type).ToList();

            foreach (var g in lightGroups)
            {
                foreach (var light in g)
                {
                    if(light.type == LightType.Directional)
                        DrawDirLight(ref context,ref renderingData,cmd,light);
                }
            }
        }

        private void DrawDirLight(ref ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd, Light light)
        {
            cmd.SetGlobalVector(ShaderPropertyIds._MainLightPosition, new float4(light.transform.forward,0));
            cmd.BlitTriangle(BuiltinRenderTextureType.None, ShaderPropertyIds._CameraColorAttachmentA, Feature.lightMat, 0);
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

            var colorDesc = renderingData.cameraData.cameraTargetDescriptor;
            colorDesc.depthBufferBits = 0;
            colorDesc.msaaSamples = 1;
            colorDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_SNorm;

            var motionDesc = colorDesc;
            motionDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16_SFloat;

            cmd.GetTemporaryRT(gbuffer0, colorDesc);
            cmd.GetTemporaryRT(gbuffer1, colorDesc);
            cmd.GetTemporaryRT(gbuffer2, colorDesc);
            cmd.GetTemporaryRT(gbuffer3, motionDesc);

            var depthDesc = colorDesc;
            depthDesc.colorFormat = RenderTextureFormat.Depth;
            depthDesc.depthBufferBits = 24;
            //cmd.GetTemporaryRT(ShaderPropertyIds._CameraDepthAttachment, depthDesc);

            colorTargets[0] = gbuffer0;
            colorTargets[1] = gbuffer1;
            colorTargets[2] = gbuffer2;
            colorTargets[3] = gbuffer3;

            var depthId = ShaderPropertyIdentifier._CameraDepthAttachment;

            cmd.SetRenderTarget(colorTargets, depthId);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.Execute(ref context);
        }
    }
}
