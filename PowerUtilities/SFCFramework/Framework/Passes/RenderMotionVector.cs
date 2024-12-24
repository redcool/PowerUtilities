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
        [Tooltip("create temporary rt : _MotionVectorTexture")]
        public bool isCreateMotionVectorTexture;

        [Header("Camera Motion Vectors")]
        [Tooltip("draw fullscreen triangle, fill _MotionVectorTexture")]
        public bool isRenderCameraMotionVectors = true;

        [LoadAsset("CameraMotionVector.mat")]
        public Material cameraMotionMat;
        [Tooltip("get worldPos from _WorldPosTexture or depthTexture reconstruct")]
        public bool isUseWorldPosTexture;
        [Tooltip("graphics device is gles3 ,use _WorldPosTexture")]
        public bool isUseWorldPosTextureWhenGLES3;

        //public BlendMode srcMode = BlendMode.One, dstMode = BlendMode.Zero;
        //[EnumSearchable(enumType = typeof(BlendOp))]
        //public BlendOp blendOp = BlendOp.Max;

        [Header("Object Motion Vectors")]
        [Tooltip("draw scene once, get object motions,fill _MotionVectorTexture,need objectMotionMaterial")]
        public bool isDrawObjectMotionVectors;
        [Tooltip("use override material otherwise use object material, pass name : MotionVectors")]
        public bool isOverrideObjectMotionMat;
        [LoadAsset("ObjectMotionVector.mat")]
        public Material objectMotionMaterial;

        public override ScriptableRenderPass GetPass()
        {
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

            if (Feature.isDrawObjectMotionVectors || Feature.isRenderCameraMotionVectors)
            {
                cmd.SetRenderTarget(ShaderPropertyIdentifier._MotionVectorTexture);
                cmd.Execute(ref context);
            }

            Matrix4x4 viewProjMatrix = default;
            var prevViewProjMatrix = MotionVectorData.Instance().GetViewProjMatrix(camera, ref viewProjMatrix);

            Shader.SetGlobalMatrix(ShaderPropertyIds._PrevViewProjMatrix, prevViewProjMatrix);
            Shader.SetGlobalMatrix(ShaderPropertyIds._ViewProjMatrix, viewProjMatrix);

            //camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth; // great importance
            
            UpdateMotionMaterial(cmd);

            if (Feature.isRenderCameraMotionVectors)
                DrawCameraMotionVectors(cmd);

            if (Feature.isDrawObjectMotionVectors)
                DrawObjectMotionVectors(ref context, ref renderingData, camera);
        }

        private void DrawObjectMotionVectors(ref ScriptableRenderContext context, ref RenderingData renderingData, Camera camera)
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
            if (Feature.isOverrideObjectMotionMat)
                drawSettings.overrideMaterial = Feature.objectMotionMaterial;

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
        }

        private void DrawCameraMotionVectors(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, Feature.cameraMotionMat, 0, MeshTopology.Triangles, 3);
        }

        private void UpdateMotionMaterial(CommandBuffer cmd)
        {
            var useWorldPosTex = Feature.isUseWorldPosTexture
                || (Feature.isUseWorldPosTextureWhenGLES3 && SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3);
            cmd.EnableShaderKeyword("USE_WORLD_POS_TEXTURE");


            //Feature.cameraMotionMat.SetFloat("_SrcMode", (int)Feature.srcMode);
            //Feature.cameraMotionMat.SetFloat("_DstMode", (int)Feature.dstMode);
            //Feature.cameraMotionMat.SetFloat("_BlendOp", (int)Feature.blendOp);
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

    }
}
