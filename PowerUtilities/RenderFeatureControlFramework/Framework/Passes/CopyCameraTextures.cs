using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+"/"+nameof(CopyCameraTextures))]
    public class CopyCameraTextures : SRPFeature
    {
        public ScriptableRenderPassInput passInputType;
        public override ScriptableRenderPass GetPass() => new CopyCameraTexturesPass(this);      
    }

    public class CopyCameraTexturesPass : SRPPass<CopyCameraTextures>
    {
        public CopyCameraTexturesPass(CopyCameraTextures feature) : base(feature)
        {
            ConfigureInput(Feature.passInputType);

        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
        }
    }
}
