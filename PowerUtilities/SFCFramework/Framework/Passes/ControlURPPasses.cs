using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// control urp pass
    /// 1 control copy pass when run
    /// 2 remove passes not want
    /// </summary>
    [Tooltip("Control Camera's Texture Execution order (Depth,Color,MotionVaetor,Normal)")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/"+nameof(ControlURPPasses))]
    public class ControlURPPasses : SRPFeature
    {
        [Header("ControlURPPasses")]
        [Tooltip("Control pass time of run")]
        public ScriptableRenderPassInput passInputType;

        [Header("Remove URP Pass")]
        public List<UniversalRendererPassTools.UrpPassType> removedPassWhenNotBaseCamera = new List<UniversalRendererPassTools.UrpPassType>();

        public List<UniversalRendererPassTools.UrpPassType> removedPass = new List<UniversalRendererPassTools.UrpPassType>();
        public override ScriptableRenderPass GetPass() => new ControlURPPassesPass(this);      
    }


    public class ControlURPPassesPass : SRPPass<ControlURPPasses>
    {
        /// <summary>
        /// remove passes when this pass run
        /// </summary>
        public List<Type> removedPassList = new List<Type>();

        public ControlURPPassesPass(ControlURPPasses feature) : base(feature)
        {
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureInput(Feature.passInputType);

            var cameraData = renderingData.cameraData;
            var renderer = cameraData.renderer;

            //remove passes not baseCamera
            if (cameraData.renderType != CameraRenderType.Base)
            {
                SetupRemoveList(Feature.removedPassWhenNotBaseCamera);
                renderer.RemoveRenderPasses(removedPassList);
            }

            //remove passes
            SetupRemoveList(Feature.removedPass);
            renderer.RemoveRenderPasses(removedPassList);
        }

        private void SetupRemoveList(List<UniversalRendererPassTools.UrpPassType> types)
        {
            removedPassList.Clear();
            foreach (var passType in types)
            {
                removedPassList.Add(UniversalRendererPassTools.GetPassType(passType));
            }
        }
    }
}
