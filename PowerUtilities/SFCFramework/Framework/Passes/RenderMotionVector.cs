using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
#if UNITY_2021_1_OR_NEWER
    [Tooltip("Control MotionVectors render,(object ,camera)")]
#endif
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/RenderMotionVector")]
    public class RenderMotionVector : SRPFeature
    {
        [Header("Texture")]
        public bool isCreateMotionVectorTexture;

        [Header("Camera Motion Vectors")]
        public bool isRenderCameraMotionVectors = true;

        [Header("--- cameraMotion material options")]
        public Material cameraMotionMat;
        public BlendMode srcMode = BlendMode.One, dstMode = BlendMode.Zero;
        public BlendOp blendOp = BlendOp.Max;

        [Header("Object Motion Vectors")]
        [Tooltip("draw scene get object motions,need objectMotionMaterial")]
        public bool isDrawObjectMotionVectors;
        public Material objectMotionMaterial;

        public override ScriptableRenderPass GetPass()
        {
            if (!cameraMotionMat)
            {
                var shader = Shader.Find("Hidden/kMotion/ObjectMotionVectors");
                if (shader)
                    cameraMotionMat = new Material(shader);
            }

            return new RenderMotionVectorPass(this);
        }
    }

    public class RenderMotionVectorPass : SRPPass<RenderMotionVector>
    {
        const GraphicsFormat motionFormat = GraphicsFormat.R16G16_SFloat;

        
        public override bool CanExecute()
        {
            var isRenderValid = (Feature.isRenderCameraMotionVectors && Feature.cameraMotionMat) || (Feature.isDrawObjectMotionVectors && Feature.objectMotionMaterial);

            return isRenderValid && base.CanExecute();
        }
        public RenderMotionVectorPass(RenderMotionVector feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            if (camera.cameraType == CameraType.Preview)
                return;

            if (Feature.isDrawObjectMotionVectors)
            {
                //ConfigureTarget(ShaderPropertyIds._MotionVectorTexture, ShaderPropertyIds._MotionVectorTexture);
                cmd.SetRenderTarget(ShaderPropertyIdentifier._MotionVectorTexture);
                cmd.Execute(ref context);
            }

            Shader.SetGlobalMatrix(ShaderPropertyIds._PrevViewProjMatrix, MotionVectorData.Instance().GetPreviousVP(camera));
            MotionVectorData.Instance().Update(camera);
            camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth; // great importance
            
            UpdateMotionMaterial();

            if (Feature.isRenderCameraMotionVectors)
                DrawCameraMotionVectors(cmd);

            if (Feature.isDrawObjectMotionVectors)
                DrawObjectMotionVectors(ref context, ref renderingData, camera);
        }

        private void DrawObjectMotionVectors(ref ScriptableRenderContext context,ref RenderingData renderingData, Camera camera)
        {
            var drawSettings = new DrawingSettings(ShaderTagIdEx.motionVectors, new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque })
            {
#if UNITY_2021_1_OR_NEWER
                fallbackMaterial = Feature.objectMotionMaterial,
#endif
                perObjectData = PerObjectData.MotionVectors,
                enableDynamicBatching = renderingData.supportsDynamicBatching,
                enableInstancing = true,
                mainLightIndex = renderingData.lightData.mainLightIndex,
            };

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
        }

        private void DrawCameraMotionVectors(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, Feature.cameraMotionMat, 0, MeshTopology.Triangles, 3);
        }

        private void UpdateMotionMaterial()
        {
            Feature.cameraMotionMat.SetFloat("_SrcMode", (int)Feature.srcMode);
            Feature.cameraMotionMat.SetFloat("_DstMode", (int)Feature.dstMode);
            Feature.cameraMotionMat.SetFloat("_BlendOp", (int)Feature.blendOp);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (Feature.isCreateMotionVectorTexture)
            {
                var desc = cameraTextureDescriptor;
                desc.graphicsFormat = motionFormat;
                cmd.GetTemporaryRT(ShaderPropertyIds._MotionVectorTexture, desc);
            }


        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            if (Feature.isCreateMotionVectorTexture)
                cmd.ReleaseTemporaryRT(ShaderPropertyIds._MotionVectorTexture);
        }

    }
}
