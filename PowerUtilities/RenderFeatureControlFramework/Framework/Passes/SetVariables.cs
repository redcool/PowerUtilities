using PowerUtilities.RenderFeatures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/SetVariables")]
    public class SetVariables : SRPFeature
    {
        public string[] floatNames;
        public float[] floatValues;

        public string[] vectorNames ;
        public Vector4[] vectorValues ;

        public override ScriptableRenderPass GetPass() => new SetVarialbesPass(this);
    }

    public class SetVarialbesPass : SRPPass<SetVariables>
    {
        public SetVarialbesPass(SetVariables feature) : base(feature)
        {
        }

        public override bool CanExecute()
        {
            return base.CanExecute() && 
                Feature.floatNames != null 
                && Feature.floatValues != null 
                && Feature.floatValues.Length == Feature.floatNames.Length;
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            Feature.floatNames.ForEach((id,index) =>
            {
                cmd.SetGlobalFloat(Feature.floatNames[index], Feature.floatValues[index]);
            });

            Feature.floatNames.ForEach((id, index) =>
            {
                cmd.SetGlobalVector(Feature.floatNames[index], Feature.vectorValues[index]);
            });

            cmd.Execute(ref context);
        }
    }
}
