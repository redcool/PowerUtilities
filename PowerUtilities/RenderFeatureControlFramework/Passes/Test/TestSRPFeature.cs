using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace PowerUtilities.RenderFeatures
{
    [CreateAssetMenu(menuName = SRP_FEATURE_MENU+"/TestSRPFeature")]
    public class TestSRPFeature : SRPFeature
    {
        public string featureName;
        public override ScriptableRenderPass GetPass()
        {
            return new TestSRPPass(this);
        }
    }
}
