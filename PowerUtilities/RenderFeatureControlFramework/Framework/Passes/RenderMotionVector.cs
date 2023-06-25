using PowerUtilities.RenderFeatures;
using System;
using System.Collections;
using System.Collections.Generic;
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
        const string kPreviousViewProjectionMatrix = "_PrevViewProjMatrix";
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
            
            MotionVectorData.Instance().Update(camera);

            if (camera.cameraType == CameraType.Preview)
                return;

            if(camera.TryGetComponent<UniversalAdditionalCameraData>(out var additionalCameraData))
            {
                var m_MotionVectorsPersistentData = typeof(UniversalAdditionalCameraData).GetField("m_MotionVectorsPersistentData", BindingFlags.Instance| BindingFlags.NonPublic);
                var motionData = m_MotionVectorsPersistentData.GetValue(additionalCameraData);
                if (motionData == null)
                {
                    return;
                }
            }
            
            Shader.SetGlobalMatrix(kPreviousViewProjectionMatrix,MotionVectorData.Instance().GetPreviousVP(camera));

            camera.depthTextureMode |= DepthTextureMode.MotionVectors | DepthTextureMode.Depth;
            DrawCameraMotionVectors(cmd);
        }

        private void DrawCameraMotionVectors(CommandBuffer cmd)
        {
            cmd.DrawProcedural(Matrix4x4.identity, Feature.cameraMotionMat, 0, MeshTopology.Triangles, 3);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var desc = cameraTextureDescriptor;
            desc.graphicsFormat = motionFormat;
            cmd.GetTemporaryRT(ShaderPropertyIds._MotionVectorTexture, desc);
            ConfigureTarget(ShaderPropertyIds._MotionVectorTexture, ShaderPropertyIds._MotionVectorTexture);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(ShaderPropertyIds._MotionVectorTexture);
        }

    }

    public class MotionVectorData
    {
        static Dictionary<Camera, Matrix4x4> dict = new Dictionary<Camera, Matrix4x4>();

        static MotionVectorData instance;
        public static MotionVectorData Instance()
        {
            if(instance == null)
            {
                instance = new MotionVectorData();
            }
            return instance;
        }

        int frameCount;
        public void Update(Camera cam)
        {
            if (Time.frameCount == frameCount)
                return;

            frameCount = Time.frameCount;

            var vp = cam.nonJitteredProjectionMatrix * cam.worldToCameraMatrix;
            dict[cam] = vp;
        }

        public Matrix4x4 GetPreviousVP(Camera cam)
        {
            if(dict.TryGetValue(cam, out var vp))
                return vp;
            else
                return cam.nonJitteredProjectionMatrix* cam.worldToCameraMatrix;
        }
    }
}
