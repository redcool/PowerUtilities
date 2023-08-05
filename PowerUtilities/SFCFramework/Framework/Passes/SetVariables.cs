namespace PowerUtilities
{
    using PowerUtilities.RenderFeatures;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Tooltip("Bind Shader varables")]
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/SetVariables")]
    public class SetVariables : SRPFeature
    {
        [Header("--- Set Variables")]
        public List<ShaderValue<float>> floatValues = new List<ShaderValue<float>>();
        public List<ShaderValue<int>> intValues = new List<ShaderValue<int>>();
        public List<ShaderValue<Vector4>> vectorValues = new List<ShaderValue<Vector4>>();

        public override ScriptableRenderPass GetPass() => new SetVarialbesPass(this);
    }

    [Serializable]
    public class ShaderValue<T> where T : struct
    {
        public string name;
        public T value;
    }

    public class SetVarialbesPass : SRPPass<SetVariables>
    {
        public SetVarialbesPass(SetVariables feature) : base(feature)
        {
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            SerupVariables(cmd, ref renderingData);
        }

        void SerupVariables(CommandBuffer cmd,ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            Feature.floatValues.ForEach(v => cmd.SetGlobalFloat(v.name, v.value));
            Feature.vectorValues.ForEach(v => cmd.SetGlobalVector(v.name, v.value));
            Feature.intValues.ForEach(v => cmd.SetGlobalInt(v.name, v.value));

            // update vars
            var isShadowMaskMixing = UniversalRenderPipeline.asset.IsLightmapShadowMixing(ref renderingData);
            cmd.SetGlobalBool(ShaderPropertyIds.shadows_ShadowMaskOn, isShadowMaskMixing);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            
        }

    }
}
