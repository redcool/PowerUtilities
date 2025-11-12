namespace PowerUtilities.RenderFeatures
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
#if UNITY_2020
    using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
    using TooltipAttribute = PowerUtilities.TooltipAttribute;
#endif
    [Tooltip("Set more color target (1 depth )to fill, 8 is max")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU + "/SetRenderTarget")]
    public class SetRenderTarget : SRPFeature
    {
        const string CLEAR_OPTIONS_GROUP = "Clear Options";
        const string SET_TARGET_GROUP = "Set Targets";
        /// ------------- set targets
        [Header(SET_TARGET_GROUP)]
        [Tooltip("Set colors and depth target,skip setTargets when not check")]
        public bool isSetTargets = true;

        [Tooltip("allocl renderTargetIdentifier per frame")]
        public bool isForceReallocRtId = false;

        [Tooltip("When empty use CurrentActive,type CameraTarget use device's cameraTarget")]
        [StringListSearchable(type = typeof(CreateRenderTarget),staticMemberName = nameof(CreateRenderTarget.GetColorTargetNames))]
        public string[] colorTargetNames = new[] { nameof(ShaderPropertyIds._CameraColorAttachmentA) };

        [Tooltip("When empty use _CameraDepthAttachment,type CameraTarget use device's cameraTarget")]
        [StringListSearchable(type = typeof(CreateRenderTarget), staticMemberName = nameof(CreateRenderTarget.GetColorTargetNames))]
        public string depthTargetName;

        /// ------------- Clear
        [EditorBorder(13,groupName = CLEAR_OPTIONS_GROUP)]
        [EditorGroup(CLEAR_OPTIONS_GROUP, true)]
        [Tooltip("clear targets before setTargets")]
        public bool clearTarget;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        [EditorHeader(CLEAR_OPTIONS_GROUP, "--- Override Clear Options")]
        [Tooltip("clear targets use camera's setting or override settings ")]
        public bool isOverrideClear;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        [Space(10)]
        [Tooltip("target 0,use clearColor,otherwise use Color.clear")]
        public bool isClearColor = true;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        public Color clearColor = Color.clear;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        [Space(10)]
        public bool isClearDepth = true;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        [Range(0, 1)] public float depth = 1;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        [Space(10)]
        public bool isClearStencil = true;

        [EditorGroup(CLEAR_OPTIONS_GROUP)]
        [Range(0, 255)] public uint stencil;

        public override ScriptableRenderPass GetPass() => new SetRenderTargetPass(this);
    }

    public class SetRenderTargetPass : SRPPass<SetRenderTarget>
    {
        RenderTargetIdentifier[] colorIds;
        RenderTargetIdentifier depthId;

        //---------- cache info
        string[] lastColorNames;
        string lastDepthName;

        RTHandle lastCameraAttachmentA;

        // trace urp 's rt changed
        bool isURPRTChanged;

        public SetRenderTargetPass(SetRenderTarget feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && camera.IsGameCamera();
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;

            //if (cameraData.isPreviewCamera)
            //{
            //    RenderTargetIdentifier cameraTarget = camera.GetCameraTarget();
            //    cmd.SetRenderTarget(cameraTarget);
            //    RenderTargetHolder.SaveTargets(new RenderTargetIdentifier[] { cameraTarget }, cameraTarget);
            //    return;
            //}

            var renderer = (UniversalRenderer)cameraData.renderer;

            TrySetTargets(ref renderingData, cmd);

            // Clear targets
            if (Feature.clearTarget)
            {
                ClearTarget(cmd, cameraData, out var needResetTargets);
                if (needResetTargets)
                {
                    TrySetTargets(ref renderingData, cmd);
                }
            }
        }

        public bool IsURPRTChanged(UniversalRenderer renderer)
        {
            var mainRT = renderer.GetCameraColorAttachmentA();

            var isSame = CompareTools.CompareAndSet(ref lastCameraAttachmentA, mainRT);
#if UNITY_EDITOR
            // sceneView,gameView, show in sametime
            isSame = false;
#endif
            return !isSame;
        }

        public void TrySetTargets(ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (Feature.isSetTargets)
            {
                SetTargets(ref renderingData, camera, cmd);
            }
        }

        void ClearTarget(CommandBuffer cmd, CameraData cameraData, out bool needResetTargets)
        {
            needResetTargets = false;

            if (Feature.isOverrideClear)
            {
                //clear all targets color(ColorClear),depth,stencil
                cmd.ClearRenderTarget(Feature.isClearColor, Color.clear, Feature.isClearDepth, Feature.depth, Feature.isClearStencil, Feature.stencil);

                //clear main target's color
                if (Feature.isSetTargets && Feature.isClearColor && Feature.clearColor != Color.clear)
                {
                    cmd.SetRenderTarget(colorIds[0]);
                    cmd.ClearRenderTarget(false, true, Feature.clearColor);

                    needResetTargets = true;
                }
            }
            else
            {
                var cam = cameraData.camera;
                cmd.ClearRenderTarget(cam);
            }
        }

        bool IsNeedAllocColorIds(UniversalRenderer renderer)
        {
            if (lastColorNames == null || lastColorNames.Length != Feature.colorTargetNames.Length)
                return true;

            for (int i = 0; i < lastColorNames.Length; i++)
            {
                if (lastColorNames[i] != Feature.colorTargetNames[i])
                    return true;
            }

            return IsURPRTChanged(renderer);
        }

        bool isNeedAllocDepthId()
        {
            if (string.IsNullOrEmpty(lastDepthName))
                return true;

            return (lastDepthName != Feature.depthTargetName) && isURPRTChanged;
        }

        void SetTargets(ref RenderingData renderingData, Camera camera, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var renderer = (UniversalRenderer)cameraData.renderer;

            var isAllocColorIds = IsNeedAllocColorIds(renderer);
            if (isAllocColorIds || Feature.isForceReallocRtId)
            {
                SetupColorIds(renderer);
            }

            //* unity 2022,urp's bug(RenderingUtils.cs line 599(TextureDesc currentRTDesc = RTHandleResourcePool.CreateTextureDesc(handle.rt.descriptor, 
            depthId = SetupDepthId(renderer);

            cmd.SetRenderTarget(colorIds, depthId);

            // keep rths, then sfc pass can use these rths.
            RenderTargetHolder.SaveTargets(colorIds, depthId);
        }

        private RenderTargetIdentifier SetupDepthId(UniversalRenderer renderer)
        {
            // set depth target id
#if UNITY_2022_1_OR_NEWER
            RenderTargetIdentifier depthId = UniversalRenderPipeline.asset.supportsCameraDepthTexture ? renderer.cameraDepthTargetHandle : BuiltinRenderTextureType.CameraTarget;
#else
            RenderTargetIdentifier depthId = UniversalRenderPipeline.asset.supportsCameraDepthTexture ? (RenderTargetIdentifier)ShaderPropertyIds._CameraDepthAttachment : BuiltinRenderTextureType.CameraTarget;
#endif
            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                depthId = Feature.depthTargetName == "CameraTarget" ? (RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget : Shader.PropertyToID(Feature.depthTargetName);
            }

            // check replace URP rtHandle
#if UNITY_2022_1_OR_NEWER
            if (depthId == ShaderPropertyIds._CameraDepthAttachment)
                renderer.TryReplaceURPRTTarget(nameof(ShaderPropertyIds._CameraDepthAttachment), ref depthId);
#endif
            // === check rt dict
            if (RenderTextureTools.TryGetRT(Feature.depthTargetName, out var rt))
                depthId = rt;

            return depthId;
        }

        public void SetupColorIds(UniversalRenderer renderer)
        {
            /**
                1 convert rtName to RenderTargetIdentifier
                2 get rt from RenderTextureTools.TryGetRT(rtName,out var rt) , createRenderTarget's result
             */
            RenderingTools.RenderTargetNameToIdentifier(Feature.colorTargetNames, ref colorIds);
#if UNITY_2022_1_OR_NEWER
            renderer.TryReplaceURPRTTargets(Feature.colorTargetNames, ref colorIds);
#endif
            // limit to 8
            if (SystemInfo.supportedRenderTargetCount < colorIds.Length)
                colorIds = colorIds.Take(SystemInfo.supportedRenderTargetCount).ToArray();

            // keep
            lastColorNames = Feature.colorTargetNames;
        }
    }
}
