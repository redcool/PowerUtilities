using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_MENU+"/"+nameof(CopyDepthWorkerFeature))]
    public class CopyDepthWorkerFeature : SRPFeature
    {
        public ScriptableRenderPassInput passInputType;
        public override ScriptableRenderPass GetPass() => new CopyDepthWorkerPass(this);      
    }
}
