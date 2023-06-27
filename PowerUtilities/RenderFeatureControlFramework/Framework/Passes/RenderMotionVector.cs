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

        public bool isCreateMotionVectorTexture;
        public Material cameraMotionMat;
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
        }

        private void DrawCameraMotionVectors(CommandBuffer cmd)
        {
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
