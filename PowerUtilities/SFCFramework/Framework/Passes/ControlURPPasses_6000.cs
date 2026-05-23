using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
using Debug = UnityEngine.Debug;

namespace PowerUtilities.RenderFeatures
{
#if UNITY_6000_4_OR_NEWER

    public class ControlURPPassesPass : SRPPass<ControlURPPasses>
    {
        /// <summary>
        /// remove passes when this pass run
        /// </summary>
        public List<Type> removedPassList = new List<Type>();

        public ControlURPPassesPass(ControlURPPasses feature) : base(feature)
        {
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            base.OnCameraSetup(cmd, ref renderingData);
            if (!CanExecute())
                return;

            RemovePasses(cmd,ref renderingData);
        }

        // use OnCameraSetup 
        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
        }

        public void RemovePasses(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(Feature.passInputType);

            var cameraData = renderingData.cameraData;
            var renderer = cameraData.renderer;

            //remove passes not baseCamera
            if (cameraData.renderType != CameraRenderType.Base)
            {
            }

            foreach (var passType in Feature.removedPass)
            {
                var pass = renderer.GetRenderPass<ScriptableRenderPass>(passType);
                pass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox + 10;
            } 
        }

    }
#endif
}
