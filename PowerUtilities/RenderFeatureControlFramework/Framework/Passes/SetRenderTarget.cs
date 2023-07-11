using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName =SRP_FEATURE_PASSES_MENU+ "/SetRenderTarget")]
    public class SetRenderTarget : SRPFeature
    {
        [Header("Targets")]
        [Tooltip("Set colors and depth target,skip setTargets when not check")]
        public bool isSetTargets = true;

        [Tooltip("use CurrentActive when item empty")]
        public string[] colorTargetNames = new[] { nameof(ShaderPropertyIds._CameraColorAttachmentA) };

        [Tooltip("use CameraTarget(or renderer.cameraDepthTarget) when empty")]
        public string depthTargetName;

        [Header("Clear ")]
        public bool clearTarget;

        [Header("--- Override Clear")]
        [Tooltip("clear target use camera's setting or settings below")]
        public bool isOverrideClear;

        [Space(10)]
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

        public SetRenderTargetPass(SetRenderTarget feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            ref var cameraData = ref renderingData.cameraData;
            var camera = renderingData.cameraData.camera;

            if (Feature.isSetTargets)
            {
                SetTargets(ref renderingData, camera, cmd);
            }

            // Clear targets
            if (Feature.clearTarget)
            {
                ClearTarget(cmd, cameraData);
            }

        }

        void ClearTarget(CommandBuffer cmd, CameraData cameraData)
        {
            if (Feature.isOverrideClear)
            {
                cmd.ClearRenderTarget(Feature.isClearColor, Feature.clearColor, Feature.isClearDepth, Feature.depth, Feature.isClearStencil, Feature.stencil);
            }
            else
            {
                var cam = cameraData.camera;
                cmd.ClearRenderTarget(cam);
            }
        }

        void SetTargets(ref RenderingData renderingData,Camera camera, CommandBuffer cmd)
        {
            if (colorIds == null || colorIds.Length != Feature.colorTargetNames.Length)
            {
                RenderingTools.RenderTargetNameToIdentifier(Feature.colorTargetNames, ref colorIds);
            }
            ref var cameraData = ref renderingData.cameraData;

            // set depth target id
            RenderTargetIdentifier depthId = UniversalRenderPipeline.asset.supportsCameraDepthTexture ? cameraData.renderer.cameraDepthTarget : BuiltinRenderTextureType.CameraTarget;
            if (!string.IsNullOrEmpty(Feature.depthTargetName))
            {
                depthId = Shader.PropertyToID(Feature.depthTargetName);
            }

            if (SystemInfo.supportedRenderTargetCount < colorIds.Length)
                colorIds = colorIds.Take(SystemInfo.supportedRenderTargetCount).ToArray();

            /// scene view show nothing
            //if (camera.IsGameCamera())
            {
                cmd.SetRenderTarget(colorIds, depthId);
            }
            
        }
    }
}
