namespace PowerUtilities.SSPR
{
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public class SSPRPass : ScriptableRenderPass
    {
        public SSPRSettingSO settings;

        int
            _TexSize = Shader.PropertyToID(nameof(_TexSize)),
            _Plane = Shader.PropertyToID(nameof(_Plane)),
            _Stretch = Shader.PropertyToID(nameof(_Stretch)),
            _FadingRange = Shader.PropertyToID(nameof(_FadingRange)),
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

        int GetWidth(int width) => (int)(width * settings.renderScale);
        int GetHeight(int height) => (int)(height * settings.renderScale);
        CommandBuffer cmd;

        bool CanRunHashPass()
        {
#if UNITY_EDITOR
            return true;
#else
                if(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 
                    || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12
                    || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal
                    || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan
                    || SystemInfoTools.IsGLES3_1Plus()
                    )
                    return true;

                //if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RInt))
                //    return true;

                return false;
#endif
        }
        /// <summary>
        /// use RWBuffer
        /// 
        /// _HashTexture
        /// 1 RWTexture<uint> : metal not support
        /// 2 Vulkan,not work on device( iqoo z3)
        /// </summary>
        /// <returns></returns>
        bool IsUseRWBuffer()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal
                || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Vulkan
                ;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cs = settings.ssprCS;
            if (!cs || !SystemInfo.supportsComputeShaders)
                return;

            if (cmd == null)
                cmd = new CommandBuffer { name = nameof(SSPRFeature) };
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
            cmd.SetComputeVectorParam(cs, _FadingRange, settings.fadingRange);

            cmd.SetComputeVectorParam(cs, _CameraTexture_TexelSize, new Vector4(desc.width, desc.height, 1f / desc.width, 1f / desc.height));

            //cmd.SetComputeShaderKeywords(cs, IsUseRWBuffer(), "TEST_BUFFER");

            var clearKernal = cs.FindKernel("CSClear");
            cs.GetKernelThreadGroupSizes(clearKernal, out var _x, out var _y, out var _z);
            var threads = new Vector2Int(Mathf.CeilToInt(width / (float)_x),
                Mathf.CeilToInt(height / (float)_y));

            switch (settings.runMode)
            {
                //case RunMode.CS_PASS_1:
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

            cmd.Execute(ref context);
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

        private void CSMain(ComputeShader cs, CommandBuffer cmd, Vector2Int threads, ScriptableRenderer renderer)
        {
            var csMain = cs.FindKernel("CSMain");
            cmd.SetComputeTextureParam(cs, csMain, _ReflectionTexture, _ReflectionTexture);
            cmd.SetComputeTextureParam(cs, csMain, _ReflectionHeightBuffer, _ReflectionHeightBuffer);

            cmd.SetComputeTextureParam(cs, csMain, _CameraDepthTexture, renderer.CameraDepthTargetHandle());
            cmd.SetComputeTextureParam(cs, csMain, _CameraOpaqueTexture, renderer.CameraColorTargetHandle());
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
            SetBufferOrTexture(cs, cmd, csHashClear);

            cmd.SetComputeTextureParam(cs, csHashClear, _ReflectionTexture, _ReflectionTexture);
            WaitDispatchCS(cs, csHashClear, cmd, threads, true);

            // hash
            var csHash = cs.FindKernel("CSHash");
            cmd.SetComputeTextureParam(cs, csHash, _CameraDepthTexture, renderer.CameraDepthTargetHandle());

            SetBufferOrTexture(cs, cmd, csHash);
            cmd.SetComputeTextureParam(cs, csHash, _ReflectionTexture, _ReflectionTexture);

            WaitDispatchCS(cs, csHash, cmd, threads, true);

            //resolve 
            var csResolve = cs.FindKernel("CSResolve");

            SetBufferOrTexture(cs, cmd, csResolve);

            cmd.SetComputeTextureParam(cs, csResolve, _ReflectionTexture, _ReflectionTexture);
            cmd.SetComputeTextureParam(cs, csResolve, _CameraOpaqueTexture, renderer.CameraColorTargetHandle());
            WaitDispatchCS(cs, csResolve, cmd, threads, true);

        }

        private void SetBufferOrTexture(ComputeShader cs, CommandBuffer cmd, int kernelId)
        {
            if (IsUseRWBuffer())
                cmd.SetComputeBufferParam(cs, kernelId, _HashResult, hashBuffer);
            else
                cmd.SetComputeTextureParam(cs, kernelId, _HashResult, _HashResult);
        }


        private void ApplyBlur(CommandBuffer cmd)
        {
            const string
                _SSPR_BLUR_SINGLE_PASS = "_SSPR_BLUR_SINGLE_PASS",
                _SSPR_OFFSET_HALF_PIXEL = "_SSPR_OFFSET_HALF_PIXEL";

            if (!settings.blurMat)
                return;

            settings.blurMat.shaderKeywords = null;

            settings.blurMat.SetFloat(_BlurSize, settings.blurSize);
            settings.blurMat.SetInt(_StepCount, settings.stepCount);

            cmd.Blit(_ReflectionTexture, _BlurReflectTex, settings.blurMat, 0);

            var isSinglePassBlur = settings.blurPassMode != BlurPassMode.TwoPasses;
            if (isSinglePassBlur)
            {
                var blurKeyword = settings.blurPassMode == BlurPassMode.OffsetHalfPixel ? _SSPR_OFFSET_HALF_PIXEL : _SSPR_BLUR_SINGLE_PASS;
                settings.blurMat.EnableKeyword(blurKeyword);

                cmd.SetGlobalTexture(_ReflectionTexture, _BlurReflectTex);
            }
            else
            {
                cmd.Blit(_BlurReflectTex, _ReflectionTexture, settings.blurMat, 1);
            }

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

            // csMain use
            if (settings.runMode == RunMode.CS_PASS_2)
            {
                desc.colorFormat = RenderTextureFormat.RFloat;
                cmd.GetTemporaryRT(_ReflectionHeightBuffer, desc);
            }

            if (settings.isApplyBlur)
            {
                var width = (int)(desc.width * settings.blurRenderScale);
                var height = (int)(desc.height * settings.blurRenderScale);
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
}