﻿using PowerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities.Features
{

    public class RenderUIPass : ScriptableRenderPass
    {
        static readonly int _GammaTex = Shader.PropertyToID(nameof(_GammaTex));
        static readonly RTHandle _GammaTexHandle = RTHandles.Alloc(_GammaTex);

        public CommandBuffer cmd = new CommandBuffer();
        /// <summary>
        /// used for framedebugger
        /// </summary>
        public string profilerName = nameof(RenderUIPass);

        public GammaUISettingSO settings;

        /// <summary>
        /// (info.isWriteTargetTextureOnce && info.targetTexture) is true,
        /// will call this callback
        /// </summary>
        public static Action OnWriteTargetTextureOnceDone;

        static NativeArray<RenderStateBlock> curRenderStateArr;

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //GameView,remove FinalBlitPass, 
            ref var cameraData = ref renderingData.cameraData;
            if (!cameraData.isSceneViewCamera && settings.isRemoveURPFinalBlit)
            {
                renderingData.cameraData.renderer.RemoveRenderPass(typeof(FinalBlitPass));
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            base.OnCameraCleanup(cmd);

            // set camera target for (ugui overlay,OnGUI)
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        }

        //[ApplicationExit]
        //[CompileStarted]
        static void DisposeNative()
        {
            if (curRenderStateArr.IsCreated)
                curRenderStateArr.Dispose();
        }

        static RenderUIPass()
        {
            ApplicationTools.OnDomainUnload += DisposeNative;

            //RenderPipelineManager.beginFrameRendering -= RenderPipelineManager_beginFrameRendering;
            //RenderPipelineManager.beginFrameRendering += RenderPipelineManager_beginFrameRendering;
        }

        private static void RenderPipelineManager_beginFrameRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            //isActiveColorTargetForceSet = false;
        }

        StencilState GetStencilState()
        {
            var stencilState = new StencilState();
            stencilState.SetCompareFunction(settings.stencilStateData.stencilCompareFunction);
            stencilState.SetFailOperation(settings.stencilStateData.failOperation);
            stencilState.SetPassOperation(settings.stencilStateData.passOperation);
            stencilState.SetZFailOperation(settings.stencilStateData.zFailOperation);
            stencilState.enabled = settings.stencilStateData.overrideStencilState;

            return stencilState;
        }

        RenderStateBlock GetRenderStateBlock()
        {
            var stencilState = GetStencilState();
            var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            if (stencilState.enabled)
            {
                renderStateBlock.stencilReference = settings.stencilStateData.stencilReference;
                renderStateBlock.stencilState = stencilState;
                renderStateBlock.mask = RenderStateMask.Stencil;
            }
            return renderStateBlock;
        }

        private void SetupURPAsset(UniversalRenderPipelineAsset urpAsset)
        {
#if UNITY_2021_1_OR_NEWER
            var isUseFSR = urpAsset.upscalingFilter == UpscalingFilterSelection.FSR;
            if (isUseFSR && settings.disableFSR)
            {
                urpAsset.upscalingFilter = UpscalingFilterSelection.Auto;
            }
#endif
        }

        bool IsWriteToCameraTargetDirect()
        {
            if (Display.main.requiresSrgbBlitToBackbuffer)
                return true;

            return settings.isWriteToCameraTargetDirectly;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            NativeArrayTools.CreateIfNull(ref curRenderStateArr, 1, Allocator.Persistent);

            var urpAsset = UniversalRenderPipeline.asset;
            SetupURPAsset(urpAsset);

            cmd.BeginSampleExecute(profilerName, ref context);
            Draw(ref context, ref renderingData);
            cmd.EndSampleExecute(profilerName, ref context);
        }
        public void Draw(ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;

#if UNITY_EDITOR
            if (cameraData.isSceneViewCamera)
            {
                DrawRenderers(ref context, ref renderingData);
                return;
            }
#endif
            // ------- wrtie to cameraTarget
            if (IsWriteToCameraTargetDirect())
            {
                DrawToCameraTarget(ref context, ref renderingData, renderer);
                return;
            }

            // ------ gamma ui and fx flow

            DrawObjectsGammaFlow(ref context, ref renderingData, cameraData);
        }

        private void DrawToCameraTarget(ref ScriptableRenderContext context, ref RenderingData renderingData, UniversalRenderer renderer)
        {
            if (Display.main.requiresSrgbBlitToBackbuffer)
            {
                ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.LinearToSRGB);
            }
            var clearFlags = settings.isClearCameraTargetDepth ? ClearFlag.Depth : ClearFlag.None;

            //1  blit current active to camera target
            if (settings.isBlitActiveColorTarget)
            {
#if UNITY_2022_1_OR_NEWER
                var curActive = renderer.cameraColorTargetHandle;
#else
                var curActive = renderer.cameraColorTarget;
#endif
                cmd.BlitTriangle(curActive, BuiltinRenderTextureType.CameraTarget, settings.blitMat, 0,
                    clearFlags: clearFlags);
            }
            else
            {
                cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                if (settings.isClearCameraTargetDepth)
                {
                    cmd.ClearRenderTarget(true, false, Color.clear);
                }
            }
            cmd.Execute(ref context);

            //2 draw object with blend
            DrawRenderers(ref context, ref renderingData);
        }

        private void DrawObjectsGammaFlow(ref ScriptableRenderContext context, ref RenderingData renderingData, CameraData cameraData)
        {
            RTHandle lastColorHandle, lastDepthHandle, colorHandle, depthHandle;
            SetupTargetTex(ref renderingData, ref cameraData, out lastColorHandle, out lastDepthHandle, out colorHandle, out depthHandle);

            // to gamma space
            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.LinearToSRGB);

            //---------------------  1 to gamma tex
            if (settings.isBlitActiveColorTarget && lastColorHandle.nameID != colorHandle.nameID)
            {
                // overwrite target
                var isClearColor = renderingData.cameraData.renderType == CameraRenderType.Base;
                BlitToTarget(ref context, lastColorHandle, colorHandle, depthHandle, isClearColor, true);
                //cmd.Execute(ref context);
            }

            //--------------------- 2 draw gamma objects
            DrawRenderers(ref context, ref renderingData);

            //--------------------- 3 to colorTarget
            // to linear space
            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.SRGBToLinear);

            GammaFlowFinalBlit(ref context, lastColorHandle, lastDepthHandle, colorHandle);

            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.None);

            cmd.Execute(ref context);
        }

        void GammaFlowFinalBlit(ref ScriptableRenderContext context, RTHandle lastColorHandle, RTHandle lastDepthHandle, RTHandle colorHandle)
        {
            switch (settings.outputTarget)
            {
                case OutputTarget.CameraTarget:
                    // write to CameraTarget
                    cmd.BlitTriangle(BuiltinRenderTextureType.CurrentActive, BuiltinRenderTextureType.CameraTarget, settings.blitMat, 0,
                        finalSrcMode: settings.finalBlitSrcMode, finalDstMode: settings.finalBlitDestMode);

                    // reset to urp targets
                    //cmd.SetRenderTarget(lastColorHandle, lastDepthHandle);
                    //cmd.Execute(ref context);
                    break;
                case OutputTarget.UrpColorTarget:
                    // write to urp target
                    BlitToTarget(ref context, colorHandle, lastColorHandle, lastDepthHandle, false, true);
                    break;
                default: break;
            }

        }

        void BlitToTarget(ref ScriptableRenderContext context, RenderTargetIdentifier lastColorHandleId, RenderTargetIdentifier colorHandleId, RenderTargetIdentifier depthHandleId, bool clearColor, bool clearDepth)
        {
            //? need twice setRenderTarget( otherwist RenderBufferLoadAction.Load not work
            cmd.SetRenderTarget(colorHandleId, depthHandleId);

            var clearFlags = ClearFlag.None;
            if (clearColor) clearFlags |= ClearFlag.Color;

#if UNITY_2021_1_OR_NEWER
            if (clearDepth) clearFlags |= ClearFlag.DepthStencil;
#else
            if (clearDepth) clearFlags |= ClearFlag.Depth;
#endif

            //cmd.Blit(BuiltinRenderTextureType.None, colorHandle, settingSO.blitMat); // will set _MainTex
            cmd.BlitTriangle(lastColorHandleId, colorHandleId, settings.blitMat, 0,
                finalSrcMode: settings.blitSrcMode,
                finalDstMode: settings.blitDestMode,
                clearFlags: clearFlags,
                depthTargetId: depthHandleId
                );
        }

        private void DrawRenderers(ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var sortFlags = SortingCriteria.CommonTransparent;

            var drawSettings = CreateDrawingSettings(ShaderTagIdEx.urpForwardShaderPassNames, ref renderingData, sortFlags);

            if (settings.isOverrideUIShader && settings.UIMaterial)
                drawSettings.overrideMaterial = settings.UIMaterial;

            //var filterSettings = new FilteringSettings(RenderQueueRange.transparent, settingSO.filterInfo.layers);
            FilteringSettings filterSettings = settings.filterInfo;
#if UNITY_EDITOR
            // When rendering the preview camera, we want the layer mask to be forced to Everything
            if (renderingData.cameraData.isPreviewCamera)
            {
                filterSettings.layerMask = -1;
            }
#endif

            var renderStateBlock = GetRenderStateBlock();
            curRenderStateArr[0] = renderStateBlock;

            // render main filter
            DrawObjectByInfo(ref filterSettings, ref context,ref renderingData, ref drawSettings, curRenderStateArr, settings.filterInfo);

            // render objects by layerMasks order
            if (settings.filterInfoList.Count > 0)
            {
                foreach (var info in settings.filterInfoList)
                {
                    if (!info.enabled)
                        continue;

                    DrawObjectByInfo(ref filterSettings, ref context,ref renderingData, ref drawSettings, curRenderStateArr, info);
                }
            }
        }

        void DrawObjectByInfo(ref FilteringSettings filterSettings, ref ScriptableRenderContext context, ref RenderingData renderingData, ref DrawingSettings drawSettings, NativeArray<RenderStateBlock> arr, FilteringSettingsInfo info)
        {
            //------------- draw objects
            var isGameCamera = renderingData.cameraData.camera.IsGameCamera();
            //1 retarget
            if (info.isRetarget && !string.IsNullOrEmpty(info.depthTargetName) && !string.IsNullOrEmpty(info.colorTargetName))
            {
                var colorId = ShaderPropertyIds.PropertyToID(info.colorTargetName);
                var depthId = ShaderPropertyIds.PropertyToID(info.depthTargetName);

                cmd.SetRenderTarget(colorId, depthId);
                if (info.clearFlags != RTClearFlags.None)
                {
                    cmd.ClearRenderTarget(info.clearFlags, Color.clear, 1, 0);
                }
                cmd.Execute(ref context);
            }
            //2 rebind targets
            foreach (var rebindTargetInfo in info.rebindTargetList)
            {
                if (rebindTargetInfo.IsValid())
                {
                    cmd.SetGlobalTexture(rebindTargetInfo.originalRTName, rebindTargetInfo.otherRTName);

                    if (isGameCamera)
                        ColorSpaceTransform.SetColorSpace(cmd, rebindTargetInfo.colorSpace);

                    cmd.Execute(ref context);
                }
            }

            //3 draw items
            filterSettings = info;
            context.DrawRenderers(cmd, renderingData.cullResults, ref drawSettings, ref filterSettings, null, arr);

            // reset 
            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.None);

            //------------- blit op
            if (info.isBlitToTarget || (info.isWriteTargetTextureOnce && info.targetTexture))
            {
                BlitToTarget(info, cmd);

                OnWriteTargetTextureOnceDone?.Invoke();
            }
        }


        void BlitToTarget(FilteringSettingsInfo info, CommandBuffer cmd)
        {
            var srcId = ShaderPropertyIds.PropertyToID(info.srcName);
            var dstId = ShaderPropertyIds.PropertyToID(info.dstName);

            if (info.isWriteTargetTextureOnce && info.targetTexture)
            {
                info.isWriteTargetTextureOnce = false;
                dstId = info.targetTexture;
            }
            var mat = info.blitMat ?? settings.blitMat;
            cmd.BlitTriangle(srcId, dstId, mat, 0);
        }

        bool AnyCameraHasPostProcessing()
        {
            //return Camera.allCameras.
            //    Select(c => c.GetComponent<UniversalAdditionalCameraData>()).
            //    Any(cd => cd.renderPostProcessing);
            foreach (var cam in Camera.allCameras)
            {
                var cd = cam.GetComponent<UniversalAdditionalCameraData>();
                if (cd && cd.renderPostProcessing) return true;
            }
            return false;
        }

        private void SetupTargetTex(ref RenderingData renderingData, ref CameraData cameraData, out RTHandle lastColorHandle, out RTHandle lastDepthHandle, out RTHandle colorHandleId, out RTHandle depthHandleId)
        {
            var renderer = (UniversalRenderer)cameraData.renderer;

            lastColorHandle = renderer.CameraColorTargetHandle();

            var colorAttachmentA = renderer.GetCameraColorAttachmentA();
            var colorAttachmentB = renderer.GetCameraColorAttachmentB();
            colorHandleId = (lastColorHandle == colorAttachmentA) ? colorAttachmentB : colorAttachmentA;

            depthHandleId = lastDepthHandle = renderer.GetRTHandle(URPRTHandleNames.m_CameraDepthAttachment);

            if (settings.createFullsizeGammaTex)
            {
                //var desc = cameraData.cameraTargetDescriptor;
                var desc = new RenderTextureDescriptor(cameraData.camera.pixelWidth, cameraData.camera.pixelHeight);
                desc.msaaSamples = 1;
                desc.colorFormat = RenderTextureFormat.Default;
                desc.sRGB = false;
                desc.depthBufferBits = (int)settings.depthBufferBits;
                cmd.GetTemporaryRT(_GammaTex, desc);

                colorHandleId = depthHandleId = _GammaTexHandle;
            }
        }


    }
}
