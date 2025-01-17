using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

namespace PowerUtilities.RenderFeatures
{

    [Serializable]
    public class BlitToTargetInfo
    {
        [EditorHeader("", "--- Source")]
        [Tooltip("When empty will use Current Active")]
        [StringListSearchable(type = typeof(CreateRenderTarget), staticMemberName = nameof(CreateRenderTarget.GetColorTargetNames))]
        public string sourceName;
        public bool isShowCurMainLightShadowMapTexture;

        //[Tooltip("use whiteTexture if empty")]
        //public string sourceDepthName;

        [EditorHeader("", "--- Target")]
        [Tooltip("When empty will use Camera Target")]
        [StringListSearchable(type = typeof(CreateRenderTarget), staticMemberName = nameof(CreateRenderTarget.GetColorTargetNames))]
        public string targetName;

        [Tooltip("blit source to [_CameraAttachmentA or B],if sourceId is A, targetId will set B")]
        public bool isBlitToNextPostTarget;

        [EditorHeader("", "--- Options")]
        [Tooltip("skip this info, when uncheck")]
        public bool isEnable = true;
        // PowerShaderLib/CopyColor.mat
        [Tooltip("check (CopyColor.mat,CopyDepth.mat)")]
        [LoadAsset("CopyColor.mat")]
        public Material blitMat;
        public int blitMatPassId = 0;

        [Tooltip("use gamma, linear space")]
        public ColorSpaceTransform.ColorSpaceMode colorSpaceMode;

        [Tooltip("use blendMode")]
        public BlendMode srcMode = BlendMode.One;
        public BlendMode dstMode = BlendMode.Zero;

        [Tooltip("clear target ")]
        public ClearFlag clearFlags = ClearFlag.None;

        public void Update(string sourceName,bool isShowCurMainLightShadowMapTexture,string targetName,bool isBlitToNextPostTarget,bool isEnable,Material blitMat,int pass
            , ColorSpaceTransform.ColorSpaceMode colorSpaceMode,BlendMode srcMode,BlendMode dstMode, ClearFlag clearFlags)
        {
            this.sourceName = sourceName;
            this.isShowCurMainLightShadowMapTexture = isShowCurMainLightShadowMapTexture;
            this.targetName = targetName;
            this.isBlitToNextPostTarget = isBlitToNextPostTarget;
            this.isEnable = isEnable;
            this.blitMat = blitMat;
            this.blitMatPassId = pass;
            this.colorSpaceMode = colorSpaceMode;
            this.srcMode = srcMode;
            this.dstMode = dstMode;
            this.clearFlags = clearFlags;
        }
    }

    [Tooltip("Blit sourceName to targetName")]
    [CreateAssetMenu(menuName=SRP_FEATURE_PASSES_MENU+ "/BlitToTarget")]
    public class BlitToTarget : SRPFeature
    {
        [EditorHeader("", "--- Source")]
        [Tooltip("When empty will use Current Active")]
        [StringListSearchable(type = typeof(CreateRenderTarget), staticMemberName = nameof(CreateRenderTarget.GetColorTargetNames))]
        public string sourceName;
        public bool isShowCurMainLightShadowMapTexture;

        //[Tooltip("use whiteTexture if empty")]
        //public string sourceDepthName;

        [EditorHeader("", "--- Target")]
        [Tooltip("When empty will use Camera Target")]
        [StringListSearchable(type = typeof(CreateRenderTarget), staticMemberName = nameof(CreateRenderTarget.GetColorTargetNames))]
        public string targetName;

        [Tooltip("blit source to [_CameraAttachmentA or B],if sourceId is A, targetId will set B")]
        public bool isBlitToNextPostTarget;

        [EditorHeader("", "--- Options")]
        [Tooltip("skip this info, when uncheck")]
        public bool isEnable = true;
        // PowerShaderLib/CopyColor.mat
        [Tooltip("check (CopyColor.mat,CopyDepth.mat)")]
        [LoadAsset("CopyColor.mat")]
        public Material blitMat;
        public int blitMatPassId = 0;

        [Tooltip("use gamma, linear space")]
        public ColorSpaceTransform.ColorSpaceMode colorSpaceMode;

        [Tooltip("use blendMode")]
        public BlendMode srcMode = BlendMode.One;
        public BlendMode dstMode = BlendMode.Zero;

        [Tooltip("clear target ")]
        public ClearFlag clearFlags = ClearFlag.None;

        [EditorHeader("", "--- Blit Infos")]
        public List<BlitToTargetInfo> targetInfos = new();


        public override ScriptableRenderPass GetPass() => new BlitToTargetPass(this);

        public BlitToTarget()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        }
    }

    public class BlitToTargetPass : SRPPass<BlitToTarget>
    {
        BlitToTargetInfo defaultBlitInfo;
        public BlitToTargetPass(BlitToTarget feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;

            UpdateDefaultBlitInfo();

            StartBlit(cmd, renderer, defaultBlitInfo);

            foreach (var info in Feature.targetInfos)
            {
                if (info.isEnable)
                    StartBlit(cmd, renderer, info);
            }
        }

        public void UpdateDefaultBlitInfo()
        {
            if (defaultBlitInfo == null)
                defaultBlitInfo = new();

            defaultBlitInfo.Update(
                Feature.sourceName,
                Feature.isShowCurMainLightShadowMapTexture,
                Feature.targetName,
                Feature.isBlitToNextPostTarget,
                true,
                Feature.blitMat,
                Feature.blitMatPassId,
                Feature.colorSpaceMode,
                Feature.srcMode,
                Feature.dstMode,
                Feature.clearFlags
                );
        }

        private void StartBlit(CommandBuffer cmd, UniversalRenderer renderer,BlitToTargetInfo info)
        {
            RenderTargetIdentifier sourceId = default;
            //RenderTargetIdentifier sourceDepthId = string.IsNullOrEmpty(Feature.sourceDepthName) ? Texture2D.whiteTexture : Shader.PropertyToID(Feature.sourceDepthName);
            // === check shadowMap,currentActive, temporary target
            if (info.isShowCurMainLightShadowMapTexture)
            {
                sourceId = ShadowUtilsEx.currentMainLightShadowMapTexture;
            }
            else
            {
                sourceId = string.IsNullOrEmpty(info.sourceName) ? (RenderTargetIdentifier)BuiltinRenderTextureType.CurrentActive : Shader.PropertyToID(info.sourceName);
            }

            var cameraTarget = camera.GetCameraTarget();
            RenderTargetIdentifier targetId = string.IsNullOrEmpty(info.targetName) ? cameraTarget : Shader.PropertyToID(info.targetName);

            renderer.FindTarget(info.sourceName, ref sourceId);
            renderer.FindTarget(info.targetName, ref targetId);

            // === check post target(A,B)
            if (info.isBlitToNextPostTarget)
            {
#if UNITY_2022_1_OR_NEWER
                var attachmentA = renderer.GetCameraColorAttachmentA();
                var attachmentB = renderer.GetCameraColorAttachmentB();

                //var currentActive = renderer.GetActiveCameraColorAttachment();
                targetId = (sourceId.IsNameIdEquals(attachmentA.nameID)) ? attachmentB : attachmentA;
#else
                var isA = sourceId.IsNameIdEquals(ShaderPropertyIds._CameraColorAttachmentA);
                targetId = isA ? ShaderPropertyIds._CameraColorAttachmentB : ShaderPropertyIds._CameraColorAttachmentA;
#endif
            }

            //cmd.SetGlobalTexture("_SourceDepthTex", sourceDepthId);

            ColorSpaceTransform.SetColorSpace(cmd, info.colorSpaceMode);

            if (info.blitMat)
            {
                cmd.BlitTriangle(sourceId, targetId, info.blitMat, info.blitMatPassId,
                    finalSrcMode: info.srcMode,
                    finalDstMode: info.dstMode,
                    clearFlags: info.clearFlags);
            }
            else
            {
                cmd.Blit(sourceId, targetId);
            }

            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.None);
        }
    }
}
