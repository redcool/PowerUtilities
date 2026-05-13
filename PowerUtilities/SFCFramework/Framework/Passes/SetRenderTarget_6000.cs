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

        RTHandle[] colorHandles;
        RTHandle depthHandle;

        //---------- cache info
        string[] lastColorNames;
        string lastDepthName;

        public SetRenderTargetPass(SetRenderTarget feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && camera.IsGameCamera();
        }

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
            if(colorHandles == null || colorHandles.Length != Feature.colorTargetNames.Length)
                colorHandles = RTHandleTools.GetRTHandles(Feature.colorTargetNames.Length);

            for (int i = 0; i < Feature.colorTargetNames.Length; i++)
            {
                var colorName = Feature.colorTargetNames[i];
                if (string.IsNullOrEmpty(colorName) || colorName.StartsWith("_CameraColor"))
                {
                    colorHandles[i] = null;
                }
                else
                {
                    if (!string.IsNullOrEmpty(colorName) && RenderTextureTools.TryGetRT(colorName, out var rt))
                    {
                        if(colorHandles[i] == null || (colorHandles[i].rt != rt))  
                        colorHandles[i] = RTHandles.Alloc(rt);
                    }
                }
            }
            //if (CompareTools.CompareAndSet(ref lastDepthName,Feature.depthTargetName))
            {
                if (RenderTextureTools.TryGetRT(Feature.depthTargetName, out var rt))
                {
                    if (depthHandle == null || depthHandle.rt != rt)
                        depthHandle = RTHandles.Alloc(rt);
                }
            }

            //ConfigureTargets(colorHandles, depthHandle);
            RenderTargetHolder.SaveTargets(colorHandles, depthHandle);
        }
    }
}
#endif 