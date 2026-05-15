#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{

    public class SetRenderTargetPass : SRPPass<SetRenderTarget>
    {
        RTHandle[] colorHandles = new RTHandle[8];
        RTHandle depthHandle;

        public SetRenderTargetPass(SetRenderTarget feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && camera.IsGameCamera();
        }
        public override bool IsTryRestoreLastTargets(Camera c) => c.IsGameCamera();

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            if (!Feature.isSetTargets)
                return;

            SetTargets();
            ClearTargets(cmd,ref renderingData);
        }

        void ClearTargets(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (!Feature.clearTarget)
                return;
            if (Feature.isOverrideClear)
            {
                var flags = CommandBufferEx.GetRTClearFlags(Feature.isClearColor, Feature.isClearDepth, Feature.isClearStencil);

                ConfigureClear(flags, Feature.clearColor, Feature.depth, Feature.stencil);
            }
            else
            {
                ref var cam = ref renderingData.cameraData.camera;
                var flags = CommandBufferEx.GetRTClearFlags(cam, out var backColor);
                ConfigureClear(flags, backColor);
            }
        }
        private void SetTargets()
        { 
            for (int i = 0; i < Feature.colorTargetNames.Length; i++)
            {
                var colorName = Feature.colorTargetNames[i];
                if (string.IsNullOrEmpty(colorName) || colorName.StartsWith("_CameraColor"))
                {
                    colorHandles[i] = null;
                }
                else
                {
                    if (!string.IsNullOrEmpty(colorName) && RenderTextureTools.TryGetRT(colorName, out var colorRT))
                    {
                        if (colorHandles[i] == null || (colorHandles[i].rt != colorRT))
                        {
                            colorHandles[i]?.Release();
                            colorHandles[i] = RTHandles.Alloc(colorRT);
                            //Debug.Log($"{Feature.GetName()} alloc color handle for {colorRT}");
                        }
                    }
                }
            }

            if (RenderTextureTools.TryGetRT(Feature.depthTargetName, out var depthRT))
            {
                if (depthHandle == null || depthHandle.rt != depthRT)
                {
                    depthHandle?.Release();
                    depthHandle = RTHandles.Alloc(depthRT);
                }
            }

            RenderTargetHolder.SaveTargets(colorHandles, depthHandle);

            RenderTargetHolder.colorTargetNames = Feature.colorTargetNames;
            RenderTargetHolder.depthTargetName = Feature.depthTargetName;
        }
    }
}
#endif 