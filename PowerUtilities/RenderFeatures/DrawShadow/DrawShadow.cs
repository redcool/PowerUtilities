using PowerUtilities;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TextureResolution = PowerUtilities.TextureResolution;

public class DrawShadow : ScriptableRendererFeature
{

    class CustomRenderPass : ScriptableRenderPass
    {
        Settings settings;
        public int
            _BigShadowMap = Shader.PropertyToID(nameof(_BigShadowMap)),
            _BigShadowVP = Shader.PropertyToID(nameof(_BigShadowVP))
            
            ;

        public CustomRenderPass(Settings settings)
        {
            this.settings = settings;
        }
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = new RenderTextureDescriptor((int)settings.res, (int)settings.res, GraphicsFormat.None, GraphicsFormatUtility.GetDepthStencilFormat(24, 0));
            desc.shadowSamplingMode = ShadowSamplingMode.CompareDepths;
            cmd.GetTemporaryRT(_BigShadowMap, desc, FilterMode.Bilinear);

            //ConfigureTarget(RTHandleTools.GetRTHandle(_BigShadowMap));
            //ConfigureClear(ClearFlag.Depth, Color.clear);

            cmd.SetRenderTarget(_BigShadowMap);
            cmd.ClearRenderTarget(true, false, Color.clear);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

            var drawSettings = CreateDrawingSettings(RenderingTools.urpForwardShaderPassNames, ref renderingData, SortingCriteria.CommonOpaque);
            drawSettings.overrideMaterial = settings.shadowMat;
            drawSettings.overrideMaterialPassIndex = 0;

            var filterSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layers);

            var cmd = CommandBufferPool.Get();
            cmd.BeginSampleExecute(nameof(DrawShadow), ref context);

            float4x4 view, proj;
            SetupVp(cmd,out view, out proj);

            cmd.SetViewport(new Rect(0, 0, (int)settings.res, (int)settings.res));
            cmd.SetViewProjectionMatrices(view, proj);

            cmd.Execute(ref context);

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

            

            cmd.DisableScissorRect();
            cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());

            cmd.EndSampleExecute(nameof(DrawShadow), ref context);
            CommandBufferPool.Release(cmd);
        }

        private void SetupVp(CommandBuffer cmd, out float4x4 view, out float4x4 proj)
        {
            var rot = Quaternion.Euler(settings.rot);
            var forward = rot*Vector3.forward;


            view =float4x4.LookAt(settings.pos, settings.pos + forward, settings.up);
            view = math.fastinverse(view);

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                //view = math.mul(float4x4.Scale(1, 1, -1), view);
                view[0][2] *=-1;
                view[1][2] *=-1;
                view[2][2] *=-1;
                view[3][2] *=-1;
            }

            var aspect = 1;
            proj =float4x4.Ortho(settings.orthoSize * aspect*2, settings.orthoSize*2, settings.near, settings.far);
            //Debug.Log((Matrix4x4)proj+"\n"+GL.GetGPUProjectionMatrix(proj, false) +"\n"+GL.GetGPUProjectionMatrix(proj, true));
            //proj = GL.GetGPUProjectionMatrix(proj, false); //z from [-1,1] -> [0,1] and reverse z

            // same as GL.GetGPUProjectionMatrix
            if (SystemInfo.usesReversedZBuffer)
            {
                //proj[0][2] *= -1;
                //proj[1][2] *= -1;
                proj[2][2] = (proj[2][2] * -0.5f);
                proj[3][2] = (proj[3][2] + 0.5f);
            }
            

            CalcShadowTransform(cmd, view, proj,forward);
        }

        float4 CalcShadowBias(float4x4 proj)
        {
            var texelSize = 2f / proj[0][0] / (int)settings.res;
            var bias = new float4(-settings.shadowDepthBias,-settings.shadowNormalBias,0,0);
            bias *= texelSize;
            return bias;
        }

        private void CalcShadowTransform(CommandBuffer cmd, float4x4 view, float4x4 proj,float3 forward)
        {
            // reverse row2 again, dont need reverse  in shader
            if (SystemInfo.usesReversedZBuffer)
            {
                proj[0][2] *= -1;
                proj[1][2] *= -1;
                proj[2][2] *= -1;
                proj[3][2] *= -1;
            }
            var halfVec = new float3(.5f, .5f, .5f);

            var m2 = float4x4.Scale(halfVec);
            var m1 = float4x4.Translate(halfVec);
            var m = math.mul(m1, m2);

            cmd.SetGlobalMatrix(_BigShadowVP, math.mul(m, math.mul(proj, view)));
            cmd.SetGlobalVector(ShaderPropertyIds._LightDirection, new float4(-forward,0));
            cmd.SetGlobalVector(ShaderPropertyIds._ShadowBias, CalcShadowBias(proj));
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_BigShadowMap);
        }

    }

    CustomRenderPass m_ScriptablePass;

    [Serializable]
    public class Settings
    {
        public TextureResolution res = TextureResolution.x512;
        public Material shadowMat;
        public LayerMask layers = 0;
        [Header("Light Camera")]
        public string lightTag = "BigShadowLight";

        public Vector3 pos, rot, up = Vector3.up;

        public float orthoSize = 2;
        public float near = 0.3f;
        public float far = 100;

        [Min(0)] public float shadowDepthBias=1, shadowNormalBias=1;
    }
    public Settings settings = new Settings();

    public GameObject lightObj;
    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(settings);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingShadows;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        if (!cameraData.camera.CompareTag("MainCamera") && !cameraData.isSceneViewCamera)
            return;

        TrySetupLightCameraInfo(cameraData.camera);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    private void TrySetupLightCameraInfo(Camera mainCam)
    {
        if (!lightObj && !string.IsNullOrEmpty(settings.lightTag))
        {
            lightObj = GameObject.FindGameObjectWithTag(settings.lightTag);
            if (!lightObj)
                return;
        }
        settings.pos = lightObj.transform.position;
        settings.rot = lightObj.transform.eulerAngles;
        settings.up = lightObj.transform.up;
    }
}


