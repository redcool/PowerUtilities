using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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

        [Tooltip("stop working when a blit finish")]
        public bool isBlitOnce;
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

        public int blitId = -1;

        public void Update(string sourceName,bool isShowCurMainLightShadowMapTexture,string targetName,bool isBlitToNextPostTarget,bool isEnable,Material blitMat,int pass
            , ColorSpaceTransform.ColorSpaceMode colorSpaceMode,BlendMode srcMode,BlendMode dstMode, ClearFlag clearFlags,bool isBlitOnce)
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
            this.isBlitOnce = isBlitOnce;
        }
    }

    [Tooltip("Blit sourceName to targetName")]
    [CreateAssetMenu(menuName=SRP_FEATURE_PASSES_MENU+ "/BlitToTarget")]
    public class BlitToTarget : SRPFeature
    {
        const string VIEWPORT_GROUP = "Viewport";

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

        [Tooltip("stop working when a blit finish")]
        [EditorIntent(1)]
        public bool isBlitOnce;
        // PowerShaderLib/CopyColor.mat
        [EditorIntent(-1)]
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

        [EditorGroup(VIEWPORT_GROUP,isHeader =true)]
        [Tooltip("override viewport rect")]
        public bool isOverrideViewport;

        [EditorGroup(VIEWPORT_GROUP)]
        public Rect viewPortRect = new Rect(0, 0, 1, 1);

        [EditorGroup(VIEWPORT_GROUP)]
        [Tooltip("use uv space[0,1] * camera.pixelSize,otherwise use pixel coordinate")]
        public bool isUseUVSpace = true;

        [EditorHeader("", "--- Other Blits")]
        [Tooltip("more blit")]
        public List<BlitToTargetInfo> otherBlitToTargetInfos = new();
        /// <summary>
        /// 
        /// </summary>
        public static Action<BlitToTarget,BlitToTargetInfo> OnBlitFinish;

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

        public override void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplicationTools.OnEnterEditMode -= BlitOnce;
            EditorApplicationTools.OnEnterEditMode += BlitOnce;

            //editor build done will change scene
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
#endif
        }
        public override void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplicationTools.OnEnterEditMode -= BlitOnce;
            EditorBuildProcessor.OnPostBuild -= BlitOnce;
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChanged;
#endif
        }

        void OnSceneChanged(Scene s, Scene s2)
        {
            BlitOnce();
        }

        public void BlitOnce()
        {
            if (Feature.isBlitOnce)
            {
                Feature.isEnable = true;
            }
        }


        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;

            UpdateDefaultBlitInfo();

            if (defaultBlitInfo.isEnable)
            {
                StartBlit(cmd, renderer, defaultBlitInfo);

                if (Feature.isBlitOnce)
                    Feature.isEnable = false;
            }

            //foreach (var info in Feature.otherBlitToTargetInfos)
            for (int i = 0; i < Feature.otherBlitToTargetInfos.Count; i++)
            {
                var info = Feature.otherBlitToTargetInfos[i];
                info.blitId = i;
                if (!info.isEnable)
                    continue;

                StartBlit(cmd, renderer, info);

                if (info.isBlitOnce)
                    info.isEnable = false;
            }
        }

        public void UpdateDefaultBlitInfo()
        {
            if (defaultBlitInfo == null)
                defaultBlitInfo = new();

            defaultBlitInfo.blitId = -1;

            defaultBlitInfo.Update(
                Feature.sourceName,
                Feature.isShowCurMainLightShadowMapTexture,
                Feature.targetName,
                Feature.isBlitToNextPostTarget,
                Feature.isEnable,
                Feature.blitMat,
                Feature.blitMatPassId,
                Feature.colorSpaceMode,
                Feature.srcMode,
                Feature.dstMode,
                Feature.clearFlags,
                Feature.isBlitOnce
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

            Rect viewportRect = default;
            if (Feature.isOverrideViewport)
            {
                viewportRect = Feature.viewPortRect;
                if (Feature.isUseUVSpace)
                {
                    var rect = new Rect(camera.pixelRect.size, camera.pixelRect.size);
                    viewportRect = Feature.viewPortRect.Mul(rect);
                }
            }

            if (info.blitMat)
            {
                cmd.BlitTriangle(sourceId, targetId, info.blitMat, info.blitMatPassId,
                    finalSrcMode: info.srcMode,
                    finalDstMode: info.dstMode,
                    clearFlags: info.clearFlags, 
                    viewPortRect:viewportRect);
            }
            else
            {
                cmd.Blit(sourceId, targetId);
            }

            ColorSpaceTransform.SetColorSpace(cmd, ColorSpaceTransform.ColorSpaceMode.None);

            // call event
            BlitToTarget.OnBlitFinish?.Invoke(Feature, info);
        }

    }
}
