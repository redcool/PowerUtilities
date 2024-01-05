namespace PowerUtilities
{
    using PowerUtilities.RenderFeatures;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
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
        public List<ShaderValue<Texture>> textureValues = new List<ShaderValue<Texture>>();

        [Header("auto variables")]
        [Tooltip("override isAutoSetShadowMask")]
        public bool isOverrideAutoSetShadowMask;
        [Tooltip("get ForwardLights and set _Shadows_ShadowMaskOn")]
        public bool isAutoSetShadowMask;

        [Header("Set MainCamera Info")]
        [Tooltip("override isSetMainCameraInfo")]
        public bool isOverrideSetMainCameraInfo;

        [Tooltip("setup camera's rotation matrix(_CameraRot,_CameraYRot),Bill.shader use these")]
        public bool isSetMainCameraInfo = true;

        [Header("set unscaled time")]
        [Tooltip("override isOverrideUnscaledTime")]
        public bool isOverrideUnscaledTime;

        [Tooltip("replace _Time(xyzw) ,use Time.unscaledTime")]
        public bool isSetUnscaledTime;

        [Header("_MIN_VERSION")]
        [Tooltip("override isMinVersionOn")]
        public bool isOverrideMinVersion;
        [Tooltip("open MIN_VERSION keyword")]
        public bool isMinVersionOn;

        public override ScriptableRenderPass GetPass() => new SetVarialbesPass(this);
    }

    [Serializable]
    public class ShaderValue<T> 
    {
        public string name;
        public T value;

        public bool IsValid => !string.IsNullOrEmpty(name) && (typeof(T).IsClass ? value != null : true);
    }


    public class SetVarialbesPass : SRPPass<SetVariables>
    {
        public SetVarialbesPass(SetVariables feature) : base(feature) { }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            SerupVariables(cmd, ref renderingData);
        }

        void SerupVariables(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;

            foreach (var v in Feature.floatValues)
                if (v.IsValid) cmd.SetGlobalFloat(v.name, v.value);

            foreach (var v in Feature.vectorValues)
                if (v.IsValid) cmd.SetGlobalVector(v.name, v.value);

            foreach (var v in Feature.intValues)
                if (v.IsValid) cmd.SetGlobalInt(v.name, v.value);

            foreach (var v in Feature.textureValues)
                if (v.IsValid) cmd.SetGlobalTexture(v.name, v.value);

            // update vars
            if (Feature.isOverrideAutoSetShadowMask && Feature.isAutoSetShadowMask)
            {
                var isShadowMaskMixing = UniversalRenderPipeline.asset.IsLightmapShadowMixing(ref renderingData);
                cmd.SetGlobalBool(ShaderPropertyIds.shadows_ShadowMaskOn, isShadowMaskMixing);
            }

            if(Feature.isOverrideSetMainCameraInfo && Feature.isSetMainCameraInfo)
            {
                var cam = Camera.main;
                if(cam)
                {
                    cmd.SetGlobalMatrix("_CameraRot",Matrix4x4.Rotate(Quaternion.Euler(cam.transform.eulerAngles)));
                    cmd.SetGlobalMatrix("_CameraYRot",Matrix4x4.Rotate(Quaternion.Euler(0,cam.transform.eulerAngles.y,0)));
                }
            }

            if(Feature.isOverrideMinVersion)
                UpdateMinVersion();
        }

        private void UpdateMinVersion()
        {
            ShaderEx.SetKeywords(Feature.isMinVersionOn, ShaderKeywords.MIN_VERSION);
        }

        public static void SetShaderTimeValues(CommandBuffer cmd, float time, float deltaTime, float smoothDeltaTime)
        {
            //float timeEights = time / 8f;
            //float timeFourth = time / 4f;
            //float timeHalf = time / 2f;

            // Time values
            Vector4 timeVector = time * new Vector4(1f / 20f, 1f, 2f, 3f);
            //Vector4 sinTimeVector = new Vector4(Mathf.Sin(timeEights), Mathf.Sin(timeFourth), Mathf.Sin(timeHalf), Mathf.Sin(time));
            //Vector4 cosTimeVector = new Vector4(Mathf.Cos(timeEights), Mathf.Cos(timeFourth), Mathf.Cos(timeHalf), Mathf.Cos(time));
            //Vector4 deltaTimeVector = new Vector4(deltaTime, 1f / deltaTime, smoothDeltaTime, 1f / smoothDeltaTime);
            //Vector4 timeParametersVector = new Vector4(time, Mathf.Sin(time), Mathf.Cos(time), 0.0f);

            cmd.SetGlobalVector(ShaderPropertyIds.time, timeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.sinTime, sinTimeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.cosTime, cosTimeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.deltaTime, deltaTimeVector);
            //cmd.SetGlobalVector(ShaderPropertyIds.timeParameters, timeParametersVector);
        }

        public override void OnExecute(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd)
        {
            if (Feature.isOverrideUnscaledTime && Feature.isSetUnscaledTime)
            {
                SetShaderTimeValues(cmd, Time.unscaledTime, Time.unscaledDeltaTime, Time.smoothDeltaTime);
            }
        }

    }
}
