using PowerUtilities.RenderFeatures;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/RenderMotionVector")]
    public class RenderMotionVector : SRPFeature
    {
        [Header("Camera Motion Vectors")]
        public bool isCreateMotionVectorTexture;
        public Material cameraMotionMat;
        public BlendMode srcMode = BlendMode.One, dstMode = BlendMode.Zero;

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
            return Feature.cameraMotionMat && base.CanExecute();
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

            Shader.SetGlobalMatrix(ShaderPropertyIds._PrevViewProjMatrix, MotionVectorData.Instance().GetPreviousVP(camera));
            MotionVectorData.Instance().Update(camera);

            camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
            DrawCameraMotionVectors(cmd);
            cmd.Execute(ref context);

            if(Feature.isDrawObjectMotionVectors && Feature.objectMotionMaterial) 
            DrawObjectMotionVectors(ref context, ref renderingData, camera);
        }

        private void DrawObjectMotionVectors(ref ScriptableRenderContext context,ref RenderingData renderingData, Camera camera)
        {
            var drawSettings = new DrawingSettings(new ShaderTagId("MotionVectors"), new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque })
            {
                fallbackMaterial = Feature.objectMotionMaterial,
                perObjectData = PerObjectData.MotionVectors,
                enableDynamicBatching = renderingData.supportsDynamicBatching,
                enableInstancing = true,
            };
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, camera.cullingMask);
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
        }

        private void DrawCameraMotionVectors(CommandBuffer cmd)
        {
            Feature.cameraMotionMat.SetFloat("_SrcMode", (int)Feature.srcMode);
            Feature.cameraMotionMat.SetFloat("_DstMode", (int)Feature.dstMode);

            cmd.DrawProcedural(Matrix4x4.identity, Feature.cameraMotionMat, 0, MeshTopology.Triangles, 3);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (Feature.isCreateMotionVectorTexture)
            {
                var desc = cameraTextureDescriptor;
                desc.graphicsFormat = motionFormat;
                cmd.GetTemporaryRT(ShaderPropertyIds._MotionVectorTexture, desc);
            }

            ConfigureTarget(ShaderPropertyIds._MotionVectorTexture, ShaderPropertyIds._MotionVectorTexture);
        }

        public override void OnFinishCameraStackRendering(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(ShaderPropertyIds._MotionVectorTexture);
        }

    }
}
