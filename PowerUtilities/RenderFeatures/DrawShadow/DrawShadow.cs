using PowerUtilities;
using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
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
            _BigShadowVP = Shader.PropertyToID(nameof(_BigShadowVP));

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
            cmd.GetTemporaryRT(_BigShadowMap, (int)settings.res, (int)settings.res, 32, FilterMode.Point, RenderTextureFormat.Depth);

            ConfigureTarget(_BigShadowMap);
            ConfigureClear(ClearFlag.Depth, Color.clear);

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
            var rot = Quaternion.Euler(settings.rot);

            var view = float4x4.LookAt(settings.pos, settings.pos + rot*Vector3.forward, settings.up);
            view = float4x4.Scale(1, 1, -1) * math.inverse(view);

            var aspect = (float)Screen.width/ Screen.height;

            var proj = float4x4.Ortho(settings.orthoSize * aspect*2, settings.orthoSize*2, settings.near, settings.far);
            proj = GL.GetGPUProjectionMatrix(proj, false);

            CalcShadowTransform(cmd, view, proj);

            cmd.SetViewProjectionMatrices(view, proj);
            cmd.Execute(ref context);

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

            cmd.SetViewProjectionMatrices(cameraData.GetViewMatrix(), cameraData.GetProjectionMatrix());
            cmd.EndSampleExecute(nameof(DrawShadow), ref context);
            CommandBufferPool.Release(cmd);
        }

        private void CalcShadowTransform(CommandBuffer cmd, float4x4 view, float4x4 proj)
        {
            var halfVec = new float3(.5f, .5f, .5f);

            var m1 = float4x4.Translate(halfVec);
            var m2 = float4x4.Scale(halfVec);
            var m = math.mul(m1, m2);

            cmd.SetGlobalMatrix(_BigShadowVP, m*proj*view);
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
        //if (!renderingData.cameraData.camera.CompareTag("MainCamera"))
        //    return;
        
        TrySetupLightCameraInfo();
        renderer.EnqueuePass(m_ScriptablePass);
    }

    private void TrySetupLightCameraInfo()
    {
        if (!lightObj && !string.IsNullOrEmpty(settings.lightTag))
        {
            var go = GameObject.FindGameObjectWithTag(settings.lightTag);
            if (go)
            {
                settings.pos = go.transform.position;
                settings.rot = go.transform.eulerAngles;
                settings.up = go.transform.up;
            }
        }
    }
}


