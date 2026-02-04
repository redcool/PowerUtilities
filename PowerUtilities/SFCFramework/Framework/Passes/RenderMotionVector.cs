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
        public string motionVectorTextureName = "_MotionVectorTexture";

        //[Header("RenderTarget")]
        //public bool isSetRenderTargetTo_MotionVectorTexture;

        [Header("Camera Motion Vectors")]
        [Tooltip("draw fullscreen triangle, fill _MotionVectorTexture")]
        public bool isRenderCameraMotionVectors = true;

        [LoadAsset("CameraMotionVector.mat")]
        public Material cameraMotionMat;

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
        RenderTexture motionTexture;
        Matrix4x4 _PrevVP,_PrevIVP;
        
        public override bool CanExecute()
        {
            var isRenderValid = (Feature.isRenderCameraMotionVectors && Feature.cameraMotionMat) || (Feature.isDrawObjectMotionVectors && Feature.objectMotionMaterial);
            return isRenderValid && !camera.IsPreviewCamera() && base.CanExecute();
        }
        public RenderMotionVectorPass(RenderMotionVector feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = cameraData.camera;

            //try create rt
            if(Feature.isCreateMotionVectorTexture)
                CreateMotionTexture(cmd, cameraData.cameraTargetDescriptor);

            //get rt
            RenderTextureTools.TryGetRT(Feature.motionVectorTextureName,out motionTexture);
            //set target and clearTarget
            cmd.SetRenderTarget(motionTexture);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.Execute(ref context);
            



            //camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth; // great importance
            
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
            var viewProjMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) * camera.worldToCameraMatrix;
            //Debug.Log(viewProjMatrix != _PrevVP);

            Shader.SetGlobalMatrix(ShaderPropertyIds._PrevViewProjMatrix, _PrevVP);
            Shader.SetGlobalMatrix(ShaderPropertyIds._ViewProjMatrix, viewProjMatrix);
            Shader.SetGlobalMatrix(nameof(_PrevIVP), _PrevVP.inverse);

            cmd.DrawProcedural(Matrix4x4.identity, Feature.cameraMotionMat, 0, MeshTopology.Triangles, 3);
            //cmd.Execute(ref context);

            // keep previous view-projection matrix
            _PrevVP = viewProjMatrix;
        }

        public void CreateMotionTexture(CommandBuffer cmd, RenderTextureDescriptor desc)
        {
            desc.graphicsFormat = motionFormat;
            desc.depthBufferBits = 0;
            RenderTextureTools.CreateRT(ref motionTexture, desc, Feature.motionVectorTextureName, FilterMode.Bilinear);
        }

    }
}
