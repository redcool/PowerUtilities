using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
/// <summary>
/// implements Screen Space Planar Reflection
/// 
/// references: 
/// https://remi-genin.github.io/posts/screen-space-planar-reflections-in-ghost-recon-wildlands/
/// https://github.com/Steven-Cannavan/URP_ScreenSpacePlanarReflections
/// https://www.cnblogs.com/idovelemon/p/13184970.html
/// 
/// </summary>
namespace PowerUtilities
{
    public class SSPRFeature : ScriptableRendererFeature
    {
        public enum RunMode
        {
            Hash, // cspass(Hash,Hash Resolve)
            CS_PASS_2,// cspass(csMain,csMain)
            CS_PASS_1 // cspass(csMain)
        }

        [Serializable]
        public class Settings
        {
            public ComputeShader ssprCS;

            [Header("Options")]
            public string reflectionTextureName = "_ScreenSpacePlanarReflectionTexture";
            public Vector3 planeLocation = Vector3.zero;
            public Quaternion planeRotation = Quaternion.identity;

            [Header("Key Options")]
            public RunMode runMode;
            public bool isFixedHoleInHashMode;

            [Range(0,2)]public int downSamples;
            public bool isApplyBlur;

            [Header("Stretch Options")]
            public bool isApplyStretch;
            public float stretchThreshold = 0.95f, stretchIntensity = 1;
        }
        class CustomRenderPass : ScriptableRenderPass
        {
            public Settings settings;

            int _TexSize = Shader.PropertyToID(nameof(_TexSize));
            int _Plane = Shader.PropertyToID(nameof(_Plane));
            int _Stretch = Shader.PropertyToID(nameof(_Stretch));

            int _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture));
            int _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA));
            int _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture));
            int _ScreenSpacePlanarReflectionTexture = Shader.PropertyToID(nameof(_ScreenSpacePlanarReflectionTexture));
            int _ReflectionHeightBuffer = Shader.PropertyToID(nameof(_ReflectionHeightBuffer));

            // hash mode
            int _HashResult = Shader.PropertyToID(nameof(_HashResult));
            // states
            int _FixedHole = Shader.PropertyToID(nameof(_FixedHole));

            const int THREAD_X = 8, THREAD_Y = 8, THREAD_Z = 1;

            Material hashResolveMat;
            int _BlurReflectTex = Shader.PropertyToID(nameof(_BlurReflectTex));

            int GetWidth(int width) => width >> settings.downSamples;
            int GetHeight(int height) => height>> settings.downSamples;

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cs = settings.ssprCS;
                if (!cs)
                    return;

                var csMain = cs.FindKernel("CSMain");
                var csClear = cs.FindKernel("CSClear");
                var csHash = cs.FindKernel("CSHash");
                var csResolve = cs.FindKernel("CSResolve");

                var cmd = CommandBufferPool.Get();

                ExecuteCommand(context, cmd);

                ref var cameraData = ref renderingData.cameraData;
                var renderer = cameraData.renderer;
                var desc = cameraData.cameraTargetDescriptor;

                var plane = settings.planeRotation * Vector3.up;

                var width = GetWidth(desc.width);
                var height = GetHeight(desc.height);
                var texSize = new Vector4(width, height, 1f / width, 1f / height);

                cmd.SetComputeVectorParam(cs, _TexSize, texSize);
                cmd.SetComputeVectorParam(cs, _Plane, new Vector4(plane.x, plane.y, plane.z, -Vector3.Dot(settings.planeLocation, plane)));
                cmd.SetComputeVectorParam(cs, _Stretch, new Vector4(cameraData.camera.transform.forward.z,
                    settings.stretchThreshold,
                    settings.stretchIntensity,
                    settings.isApplyStretch ? 1 : 0));

                cmd.SetComputeIntParam(cs, _FixedHole, settings.isFixedHoleInHashMode ? 1 : 0);

                var threads = new Vector2Int(Mathf.CeilToInt(width / (float)THREAD_X),
                    Mathf.CeilToInt(height / (float)THREAD_Y));

                // clear
                cmd.SetComputeTextureParam(cs, csClear, _ScreenSpacePlanarReflectionTexture, _ScreenSpacePlanarReflectionTexture);
                cmd.SetComputeTextureParam(cs, csClear, _ReflectionHeightBuffer, _ReflectionHeightBuffer);
                cmd.SetComputeTextureParam(cs, csClear, _HashResult, _HashResult);
                cmd.DispatchCompute(cs, csClear, threads.x, threads.y, 1);

                //main
                cmd.SetComputeTextureParam(cs, csMain, _ScreenSpacePlanarReflectionTexture, _ScreenSpacePlanarReflectionTexture);
                cmd.SetComputeTextureParam(cs, csMain, _ReflectionHeightBuffer, _ReflectionHeightBuffer);

                cmd.SetComputeTextureParam(cs, csMain, _CameraDepthTexture, _CameraDepthTexture);
                cmd.SetComputeTextureParam(cs, csMain, _CameraOpaqueTexture, renderer.cameraColorTarget);
                cmd.SetComputeTextureParam(cs, csMain, _HashResult, _HashResult);


                switch (settings.runMode)
                {
                    case RunMode.CS_PASS_1:
                        // main 1
                        WaitDispatchCS(cs, csMain, cmd, threads);
                        break;
                    case RunMode.CS_PASS_2:
                        // main 1
                        WaitDispatchCS(cs, csMain, cmd, threads);
                        //main 2, reduce flickers
                        WaitDispatchCS(cs, csMain, cmd, threads);
                        break;

                    case RunMode.Hash:
                        // for (pc ,console),2 ssprCS pass
                        HashPass(cs, csHash, csResolve, cmd, renderer, threads);
                        //blit resolve hash
                        //BlitResolveHash(cmd, renderer);

                        break;

                }

                if (settings.isApplyBlur)
                {
                    ApplyBlur(cmd);
                }

                ExecuteCommand(context, cmd);
                CommandBufferPool.Release(cmd);
            }

            void HashPass(ComputeShader cs, int csHash, int csResolve, CommandBuffer cmd, ScriptableRenderer renderer, Vector2Int threads)
            {
                // hash
                cmd.SetComputeTextureParam(cs, csHash, _ReflectionHeightBuffer, _ReflectionHeightBuffer);
                cmd.SetComputeTextureParam(cs, csHash, _CameraDepthTexture, _CameraDepthTexture);
                cmd.SetComputeTextureParam(cs, csHash, _HashResult, _HashResult);

                WaitDispatchCS(cs, csHash, cmd, threads);

                //resolve 

                cmd.SetComputeTextureParam(cs, csResolve, _HashResult, _HashResult);
                cmd.SetComputeTextureParam(cs, csResolve, _ScreenSpacePlanarReflectionTexture, _ScreenSpacePlanarReflectionTexture);
                cmd.SetComputeTextureParam(cs, csResolve, _CameraOpaqueTexture, renderer.cameraColorTarget);
                WaitDispatchCS(cs, csResolve, cmd, threads);
            }

            private void BlitResolveHash(CommandBuffer cmd, ScriptableRenderer renderer)
            {
                if (!hashResolveMat)
                    hashResolveMat = new Material(Shader.Find("Hidden/HashResolve"));
                cmd.SetGlobalTexture(_CameraOpaqueTexture, renderer.cameraColorTarget);
                cmd.Blit(_HashResult, _ScreenSpacePlanarReflectionTexture, hashResolveMat, 0); // no blur
            }

            private void ApplyBlur(CommandBuffer cmd)
            {
                cmd.Blit(_ScreenSpacePlanarReflectionTexture, _BlurReflectTex, hashResolveMat, 1);
                cmd.SetGlobalTexture(_ScreenSpacePlanarReflectionTexture, _BlurReflectTex);
            }

            private static void WaitDispatchCS(ComputeShader cs, int csId, CommandBuffer cmd, Vector2Int threads, bool needFence = false)
            {
                cmd.DispatchCompute(cs, csId, threads.x, threads.y, 1);

                if (needFence)
                {
                    var fence = cmd.CreateGraphicsFence(GraphicsFenceType.AsyncQueueSynchronisation, SynchronisationStageFlags.ComputeProcessing);
                    cmd.WaitOnAsyncGraphicsFence(fence);
                }
            }

            private static void ExecuteCommand(ScriptableRenderContext context, CommandBuffer cmd)
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor desc)
            {
                if (!string.IsNullOrEmpty(settings.reflectionTextureName) && settings.reflectionTextureName != nameof(_ScreenSpacePlanarReflectionTexture))
                {
                    _ScreenSpacePlanarReflectionTexture = Shader.PropertyToID(settings.reflectionTextureName);
                }

                desc.width = GetWidth(desc.width);
                desc.height = GetHeight(desc.height);
                desc.msaaSamples = 1;
                desc.enableRandomWrite = true;
                desc.colorFormat = RenderTextureFormat.Default;

                cmd.GetTemporaryRT(_ScreenSpacePlanarReflectionTexture, desc, FilterMode.Bilinear);

                desc.colorFormat = RenderTextureFormat.RFloat;
                cmd.GetTemporaryRT(_ReflectionHeightBuffer, desc);

                desc.colorFormat = RenderTextureFormat.RInt;
                cmd.GetTemporaryRT(_HashResult, desc);

                if (settings.isApplyBlur)
                {
                    var width = desc.width >> (settings.downSamples ==0 ? 1 : settings.downSamples);
                    var height = desc.height >>  (settings.downSamples ==0 ? 1 : settings.downSamples);
                    cmd.GetTemporaryRT(_BlurReflectTex, width, height, 0, FilterMode.Bilinear);
                }

            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (cmd == null) return;
                cmd.ReleaseTemporaryRT(_ScreenSpacePlanarReflectionTexture);
                cmd.ReleaseTemporaryRT(_ReflectionHeightBuffer);
                cmd.ReleaseTemporaryRT(_HashResult);

                if (settings.isApplyBlur)
                {
                    cmd.ReleaseTemporaryRT(_BlurReflectTex);
                }
            }
        }

        CustomRenderPass ssprPass;
        public Settings settings;

        /// <inheritdoc/>
        public override void Create()
        {
            ssprPass = new CustomRenderPass() { settings = settings };

            // Configures where the render pass should be injected.
            ssprPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when settings up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(ssprPass);
        }
    }


}