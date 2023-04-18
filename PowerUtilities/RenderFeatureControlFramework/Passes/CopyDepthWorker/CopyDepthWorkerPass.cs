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
    public class CopyDepthWorkerPass : SRPPass<CopyDepthWorkerFeature>
    {
        public CopyDepthWorkerPass(CopyDepthWorkerFeature feature) : base(feature) { 
            ConfigureInput(Feature.passInputType);
        
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
        }
    }
}
