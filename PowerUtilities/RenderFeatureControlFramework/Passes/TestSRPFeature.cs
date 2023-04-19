using PowerUtilities.RenderFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;

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


    public class TestSRPPass : SRPPass<TestSRPFeature>
    {
        public TestSRPPass(TestSRPFeature feature) : base(feature) { }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            Debug.Log(Feature.featureName + "done");
        }
    }
}
