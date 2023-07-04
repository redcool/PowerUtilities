using System;
using System.Threading;
using UnityEditor.Rendering;
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
/// usage:
/// 1 add SSPRFeature to UniversalRenderPipelineAsset_Renderer
///     1.1 ssprFeature add ssprCore
///     1.2 change params
/// 2 add 3D plane to scene
///     2.1 assign SSPRFeature/Shaders/ShowReflectionTexture.mat to plane
///     2.2 chang plane'mat renderqueue > 2500
///     
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
            public string reflectionTextureName = "_ReflectionTexture";
            public Vector3 planeLocation = new Vector3(0,0.01f,0);
            public Quaternion planeRotation = Quaternion.identity;
            public float fading = 10;

            [Header("Stretch Options")]
            public bool isApplyStretch;
            public float stretchThreshold = 0.95f, stretchIntensity = 1;

            [Header("Key Options")]
            public RunMode runMode;
            public bool runModeAuto = true;
            public bool isFixedHoleInHashMode;

            [Header("control buffer's resolution")]
            [Range(0,2)]public int downSamples;
            [Header("Blur")]
            public bool isApplyBlur;
            public bool isSinglePass = true;
            public Material blurMat;
            [Min(1)]public float blurSize=1;
            [Range(2,10)]public int stepCount=10;

            [Header("Render")]
            public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingSkybox;
            public int renderEventOffset = 0;
            public string gameCameraTag = "MainCamera";
        }
        class SSPRPass : ScriptableRenderPass
        {
            const int THREAD_X = 32, THREAD_Y = 32, THREAD_Z = 1;

            public Settings settings;

            int 
                _TexSize = Shader.PropertyToID(nameof(_TexSize)),
                _Plane = Shader.PropertyToID(nameof(_Plane)),
                _Stretch = Shader.PropertyToID(nameof(_Stretch)),
                _Fading = Shader.PropertyToID(nameof(_Fading)),
                _BlurSize = Shader.PropertyToID(nameof(_BlurSize)),
                _StepCount = Shader.PropertyToID(nameof(_StepCount)),

                _CameraOpaqueTexture = Shader.PropertyToID(nameof(_CameraOpaqueTexture)),
                _CameraColorAttachmentA = Shader.PropertyToID(nameof(_CameraColorAttachmentA)),
                _CameraDepthTexture = Shader.PropertyToID(nameof(_CameraDepthTexture)),
                _ReflectionTexture = Shader.PropertyToID(nameof(_ReflectionTexture)),
                _ReflectionHeightBuffer = Shader.PropertyToID(nameof(_ReflectionHeightBuffer)),

                // hash mode
                _HashResult = Shader.PropertyToID(nameof(_HashResult)),
                // states
                _FixedHole = Shader.PropertyToID(nameof(_FixedHole)),
                _BlurReflectTex = Shader.PropertyToID(nameof(_BlurReflectTex)),
                _RunMode = Shader.PropertyToID(nameof(_RunMode)),
                _VP = Shader.PropertyToID(nameof(_VP)),
                _InvVP = Shader.PropertyToID(nameof(_InvVP)),
                _CameraTexture_TexelSize = Shader.PropertyToID(nameof(_CameraTexture_TexelSize)) // urp texture size
                ;

            ComputeBuffer hashBuffer;

            int GetWidth(int width) => width >> settings.downSamples;
            int GetHeight(int height) => height>> settings.downSamples;

            bool CanRunHashPass()
            {
#if UNITY_EDITOR
                return true;
#else
                if(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 
                    || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12
                    || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal
                    )
                    return true;

                //if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt))
                //    return true;

                return false;
#endif
            }

            bool IsUseRWBuffer()
            {
                //return true;
                return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cs = settings.ssprCS;
                if (!cs || !SystemInfo.supportsComputeShaders)
                    return;

                var cmd = CommandBufferPool.Get(nameof(SSPRFeature));
                //ExecuteCommand(context, cmd);

                ref var cameraData = ref renderingData.cameraData;
                var cam = cameraData.camera;
                var renderer = cameraData.renderer;
                var desc = cameraData.cameraTargetDescriptor;
                var plane = settings.planeRotation * Vector3.up;

                TryUpdateRunMode();

                // vp,invVP
                var vp = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true) * cam.worldToCameraMatrix;
                var invVP = vp.inverse;
                cmd.SetComputeMatrixParam(cs, _VP, vp);
                cmd.SetComputeMatrixParam(cs, _InvVP, invVP);

                // size
                var width = GetWidth(desc.width);
                var height = GetHeight(desc.height);
                var texSize = new Vector4(width, height, 1f / width, 1f / height);

                // bind cs vars
                cmd.SetComputeVectorParam(cs, _TexSize, texSize);
                cmd.SetComputeVectorParam(cs, _Plane, new Vector4(plane.x, plane.y, plane.z, -Vector3.Dot(settings.planeLocation, plane)));
                cmd.SetComputeVectorParam(cs, _Stretch, new Vector4(cameraData.camera.transform.forward.z,
                    settings.stretchThreshold,
                    settings.stretchIntensity,
                    settings.isApplyStretch ? 1 : 0));

                cmd.SetComputeIntParam(cs, _FixedHole, settings.isFixedHoleInHashMode ? 1 : 0);
                cmd.SetComputeIntParam(cs, _RunMode, (int)settings.runMode);
                cmd.SetComputeFloatParam(cs, _Fading, settings.fading);

                cmd.SetComputeVectorParam(cs, _CameraTexture_TexelSize, new Vector4(desc.width, desc.height));

                //cmd.SetComputeShaderKeywords(cs, IsUseRWBuffer(), "TEST_BUFFER");

                var threads = new Vector2Int(Mathf.CeilToInt(width / (float)THREAD_X),
                    Mathf.CeilToInt(height / (float)THREAD_Y));

                switch (settings.runMode)
                {
                    case RunMode.CS_PASS_1:
                    case RunMode.CS_PASS_2:
                        CSClear(cs, cmd, threads);
                        CSMain(cs, cmd, threads, renderer);

                        break;

                    case RunMode.Hash:
                        // for (pc ,console),2 ssprCS pass
                        HashPass(cs, cmd, renderer, threads);
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

            private void TryUpdateRunMode()
            {
                if (settings.runModeAuto)
                {
                    settings.runMode = CanRunHashPass() ? RunMode.Hash : RunMode.CS_PASS_2;
                }
                else
                {
                    if (settings.runMode == RunMode.Hash && !CanRunHashPass())
                    {
                        settings.runMode = RunMode.CS_PASS_2;
                    }
                }
            }

            private void CSMain(ComputeShader cs, CommandBuffer cmd,Vector2Int threads, ScriptableRenderer renderer)
            {
                var csMain = cs.FindKernel("CSMain"); 
                cmd.SetComputeTextureParam(cs, csMain, _ReflectionTexture, _ReflectionTexture);
                cmd.SetComputeTextureParam(cs, csMain, _ReflectionHeightBuffer, _ReflectionHeightBuffer);

                cmd.SetComputeTextureParam(cs, csMain, _CameraDepthTexture, _CameraDepthTexture);
                cmd.SetComputeTextureParam(cs, csMain, _CameraOpaqueTexture, renderer.cameraColorTarget);
                //// main 1
                WaitDispatchCS(cs, csMain, cmd, threads);
                ////main 2, reduce flickers
                WaitDispatchCS(cs, csMain, cmd, threads);
            }

            void CSClear(ComputeShader cs, CommandBuffer cmd, Vector2Int threads)
            {
                var csClear = cs.FindKernel("CSClear");
                cmd.SetComputeTextureParam(cs, csClear, _ReflectionTexture, _ReflectionTexture);
                cmd.SetComputeTextureParam(cs, csClear, _ReflectionHeightBuffer, _ReflectionHeightBuffer);
                WaitDispatchCS(cs, csClear, cmd, threads);
            }

            void HashPass(ComputeShader cs, CommandBuffer cmd, ScriptableRenderer renderer, Vector2Int threads)
            {
                var csHashClear = cs.FindKernel("CSHashClear");
                if (IsUseRWBuffer())
                    cmd.SetComputeBufferParam(cs, csHashClear, _HashResult, hashBuffer);
                else
                    cmd.SetComputeTextureParam(cs, csHashClear, _HashResult, _HashResult);

                cmd.SetComputeTextureParam(cs, csHashClear, _ReflectionTexture, _ReflectionTexture);
                WaitDispatchCS(cs, csHashClear, cmd, threads);

                // hash
                var csHash = cs.FindKernel("CSHash");
                cmd.SetComputeTextureParam(cs, csHash, _CameraDepthTexture, _CameraDepthTexture);

                if (IsUseRWBuffer())
                    cmd.SetComputeBufferParam(cs, csHash, _HashResult, hashBuffer);
                else
                    cmd.SetComputeTextureParam(cs, csHash, _HashResult, _HashResult);


                WaitDispatchCS(cs, csHash, cmd, threads);

                //resolve 
                var csResolve = cs.FindKernel("CSResolve");
                if (IsUseRWBuffer())
                    cmd.SetComputeBufferParam(cs, csResolve, _HashResult, hashBuffer);
                else
                    cmd.SetComputeTextureParam(cs, csResolve, _HashResult, _HashResult);

                cmd.SetComputeTextureParam(cs, csResolve, _ReflectionTexture, _ReflectionTexture);
                cmd.SetComputeTextureParam(cs, csResolve, _CameraOpaqueTexture, renderer.cameraColorTarget);
                WaitDispatchCS(cs, csResolve, cmd, threads);
            }


            private void ApplyBlur(CommandBuffer cmd)
            {
                if (!settings.blurMat)
                    return;

                settings.blurMat.SetFloat(_BlurSize, settings.blurSize);
                settings.blurMat.SetInt(_StepCount, settings.stepCount);

                cmd.Blit(_ReflectionTexture, _BlurReflectTex,settings.blurMat,0);
                if (settings.isSinglePass)
                {
                    cmd.SetGlobalTexture(_ReflectionTexture, _BlurReflectTex);
                    return;
                }
                cmd.Blit(_BlurReflectTex, _ReflectionTexture, settings.blurMat, 1);
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
                if (!string.IsNullOrEmpty(settings.reflectionTextureName) && settings.reflectionTextureName != nameof(_ReflectionTexture))
                {
                    _ReflectionTexture = Shader.PropertyToID(settings.reflectionTextureName);
                }

                desc.sRGB = false; // great importance
                desc.width = GetWidth(desc.width);
                desc.height = GetHeight(desc.height);
                desc.msaaSamples = 1;
                desc.enableRandomWrite = true;
                desc.colorFormat = RenderTextureFormat.Default;
                desc.depthBufferBits = 0;

                cmd.GetTemporaryRT(_ReflectionTexture, desc, FilterMode.Bilinear);

                desc.colorFormat = RenderTextureFormat.RInt;

                if (IsUseRWBuffer())
                {
                    if (hashBuffer != null)
                        hashBuffer.Release();
                    hashBuffer = new ComputeBuffer(desc.width * desc.height, 4);
                }
                else
                {
                    cmd.GetTemporaryRT(_HashResult, desc);
                }

                desc.colorFormat = RenderTextureFormat.RFloat;
                cmd.GetTemporaryRT(_ReflectionHeightBuffer, desc);

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
                cmd.ReleaseTemporaryRT(_ReflectionTexture);
                cmd.ReleaseTemporaryRT(_ReflectionHeightBuffer);
                cmd.ReleaseTemporaryRT(_HashResult);

                if (settings.isApplyBlur)
                {
                    cmd.ReleaseTemporaryRT(_BlurReflectTex);
                }
            }
        }

        SSPRPass ssprPass;
        public Settings settings = new Settings();

        /// <inheritdoc/>
        public override void Create()
        {
            ssprPass = new SSPRPass() { settings = settings };

            // Configures where the render pass should be injected.
            ssprPass.renderPassEvent = settings.renderEvent + settings.renderEventOffset;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when settings up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var enabled = string.IsNullOrEmpty(settings.gameCameraTag) ? true:cameraData.camera.CompareTag(settings.gameCameraTag) ;
            enabled = enabled || cameraData.cameraType != CameraType.Game;
            if (!enabled)
                return;

            renderer.EnqueuePass(ssprPass);
        }
    }


}