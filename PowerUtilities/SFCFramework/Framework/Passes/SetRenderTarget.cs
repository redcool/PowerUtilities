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

    [Tooltip("Set more color target (1 depth )to fill, 8 is max")]
    [CreateAssetMenu(menuName =SRP_FEATURE_PASSES_MENU+ "/SetRenderTarget")]
    public class SetRenderTarget : SRPFeature
    {
        [Header("Targets")]
        [Tooltip("Set colors and depth target,skip setTargets when not check")]
        public bool isSetTargets = true;

        [Tooltip("use CurrentActive when item empty")]
        public string[] colorTargetNames = new[] { nameof(ShaderPropertyIds._CameraColorAttachmentA) };

        [Tooltip("use CameraTarget or _CameraDepthAttachment urp surpport depthTexture when empty")]
        public string depthTargetName;

        [Header("Clear ")]
        public bool clearTarget;

        [Header("--- Override Clear ")]
        [Tooltip("clear target use camera's setting or settings below")]
        public bool isOverrideClear;

        [Space(10)]
        [Tooltip("target 0,use clearColor,otherwise use Color.clear")]
        public bool isClearColor = true;

        public Color clearColor = Color.clear;
       
        [Space(10)]
        public bool isClearDepth = true;
       
        [Range(0,1)] public float depth = 1;

        [Space(10)]
        public bool isClearStencil = true;
        
        [Range(0,255)]public uint stencil;

        public override ScriptableRenderPass GetPass() => new SetRenderTargetPass(this);
    }

    public class SetRenderTargetPass : SRPPass<SetRenderTarget>
    {
        RenderTargetIdentifier[] colorIds;

        //---------- cache info
        string[] lastColorNames;
        string lastDepthName;

        RenderTargetIdentifier lastTargetNameId;

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

        public bool CheckURPChangeRT(UniversalRenderer renderer)
        {
            var mainRT = renderer.GetCameraColorAttachmentA();
            var isRTChanged = (mainRT.nameID != lastTargetNameId);

            lastTargetNameId = mainRT.nameID;

            return isRTChanged;
        }

        public void TrySetTargets(ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (Feature.isSetTargets)
            {
                SetTargets(ref renderingData, camera, cmd);
            }
        }

        void ClearTarget(CommandBuffer cmd, CameraData cameraData,out bool needResetTargets)
        {
            needResetTargets = false;

            if (Feature.isOverrideClear)
            {
                //clear all targets color(ColorClear),depth,stencil
                cmd.ClearRenderTarget(Feature.isClearColor, Color.clear, Feature.isClearDepth, Feature.depth, Feature.isClearStencil, Feature.stencil);

                //clear main target's color
                if(Feature.isSetTargets && Feature.isClearColor && Feature.clearColor != Color.clear)
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

            return CheckURPChangeRT(renderer);
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
            if (isAllocColorIds)
            {
                SetupColorIds(renderer);
            }

            RenderTargetIdentifier depthId = SetupDepthId(renderer);
            cmd.SetRenderTarget(colorIds, depthId);

            // keep rths, then sfc pass can use these rths.
            RenderTargetHolder.SaveTargets(colorIds,depthId);

            // update urp renderer's default targets
            UpdateRendererDefaultTargets(renderer, depthId);
        }

        private void UpdateRendererDefaultTargets(UniversalRenderer renderer, RenderTargetIdentifier depthId)
        {
            var colorHandle = RTHandles.Alloc(colorIds[0]);
            var depthHandle = RTHandles.Alloc(depthId);
            renderer.ConfigureCameraTarget(colorHandle, depthHandle);
        }

        private RenderTargetIdentifier SetupDepthId(UniversalRenderer renderer)
        {
            // set depth target id
            RenderTargetIdentifier depthId = UniversalRenderPipeline.asset.supportsCameraDepthTexture ? ShaderPropertyIds._CameraDepthAttachment : BuiltinRenderTextureType.CameraTarget;
            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                depthId = Shader.PropertyToID(Feature.depthTargetName);
            }

            // check replace URP rtHandle
#if UNITY_2022_1_OR_NEWER
            if (depthId == ShaderPropertyIds._CameraDepthAttachment)
                renderer.TryReplaceURPRTTarget(nameof(ShaderPropertyIds._CameraDepthAttachment), ref depthId);
#endif
            return depthId;
        }

        private void SetupColorIds(UniversalRenderer renderer)
        {
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
