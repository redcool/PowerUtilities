namespace PowerUtilities.RenderFeatures
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

#if UNITY_2021_1_OR_NEWER
    [Tooltip("Bind Shader varables")]
#endif
    [CreateAssetMenu(menuName = SRP_FEATURE_PASSES_MENU+ "/SetVariables")]
    public class SetVariables : SRPFeature
    {
        [Header("--- Set Variables")]
        [Tooltip("binding global float variables")]
        [ListItemDraw("name:,name,value:,value","50,100,50,")]
        public List<ShaderValue<float>> floatValues = new List<ShaderValue<float>>();

        [ListItemDraw("name:,name,value:,value", "50,100,50,")]
        public List<ShaderValue<int>> intValues = new List<ShaderValue<int>>();

        [ListItemDraw("name:,name,value:,value", "50,100,50,")]
        public List<ShaderValue<Vector4>> vectorValues = new List<ShaderValue<Vector4>>();

        [ListItemDraw("name:,name,value:,value", "50,100,50,")]
        public List<ShaderValue<Color>> colorValues = new List<ShaderValue<Color>>();

        [ListItemDraw("name:,name,value:,value", "50,100,50,")]
        public List<ShaderValue<Texture>> textureValues = new List<ShaderValue<Texture>>();

        [Tooltip("rebind rt name(Temporary or named RenderTexture)")]
        public List<ShaderValue<string>> rtNames = new List<ShaderValue<string>>();

        [Header("auto variables")]
        [Tooltip("override isAutoSetShadowMask")]
        public bool isOverrideAutoSetShadowMask;
        [Tooltip("get ForwardLights and set _Shadows_ShadowMaskOn")]
        [EditorDisableGroup(targetPropName = "isOverrideAutoSetShadowMask")]
        public bool isAutoSetShadowMask;

        //[Header("Set MainCamera Info")]
        //[Tooltip("override isSetMainCameraInfo")]
        //public bool isOverrideSetMainCameraInfo;

        [Tooltip("setup rotation matrix(_CameraRot,_CameraYRot,_MainLightYRot),Bill.shader use these")]
        //[EditorDisableGroup(targetPropName = "isOverrideSetMainCameraInfo")]
        public bool isSetMainCameraAndLight = true;

        [Header("set unscaled time")]
        [Tooltip("override isOverrideUnscaledTime")]
        public bool isOverrideUnscaledTime;

        [Tooltip("replace _Time(xyzw) ,use Time.unscaledTime")]
        [EditorDisableGroup(targetPropName = "isOverrideUnscaledTime")]
        public bool isSetUnscaledTime;

        [Header("Global Lod Settings,effect subShader")]
        public bool isOverrideGlobalMaxLod;
        [EditorDisableGroup(targetPropName = "isOverrideGlobalMaxLod")]
        public int globalMaxLod = 600;

        [Header("MIN_VERSION")]
        [Tooltip("override isMinVersionOn")]
        public bool isOverrideMinVersion;

        [Tooltip("subshader lod [100,300]")]
        [EditorDisableGroup(targetPropName = "isOverrideMinVersion")]
        public bool isMinVersionOn;

        [Header("URP Asset")]
        [Tooltip("override urp asset's renderScale")]
        public bool isOverrideRenderScale;

        [EditorDisableGroup(targetPropName = "isOverrideRenderScale")]
        [Range(0.01f,2)] public float renderScale = 1;
        //used for check Feature.isOverriderRenderScale
        [HideInInspector]public bool lastIsOverrideRenderScale;
        public override ScriptableRenderPass GetPass() => new SetVarialbesPass(this);
    }


    public class SetVarialbesPass : SRPPass<SetVariables>
    {
        static Dictionary<RenderPipelineAsset, float> assetRenderScaleDict = new Dictionary<RenderPipelineAsset, float>();
        public SetVarialbesPass(SetVariables feature) : base(feature) { }

        Light mainLight;

        void SetupVariables(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var camera = renderingData.cameraData.camera;
            var renderer = (UniversalRenderer)renderingData.cameraData.renderer;

            SetupVariables(cmd, renderer);

            // update vars
            if (Feature.isOverrideAutoSetShadowMask && Feature.isAutoSetShadowMask)
            {
                var isShadowMaskMixing = UniversalRenderPipeline.asset.IsLightmapShadowMixing(ref renderingData);
                cmd.SetGlobalBool(ShaderPropertyIds.shadows_ShadowMaskOn, isShadowMaskMixing);
            }

            //if(Feature.isOverrideSetMainCameraInfo)
            {
                SetupCameraLightRotateMatrix(cmd, Camera.main, LightTools.FindMainLight(false,Tags.BigShadowLight));
            }

            if (Feature.isOverrideGlobalMaxLod)
                Shader.globalMaximumLOD = Feature.globalMaxLod;

            if (Feature.isOverrideMinVersion)
                UpdateMinVersion();

            if (Feature.isOverrideUnscaledTime && Feature.isSetUnscaledTime)
            {
                SetShaderTimeValues(cmd, Time.unscaledTime, Time.unscaledDeltaTime, Time.smoothDeltaTime);
            }
        }

        private void SetupVariables(CommandBuffer cmd, UniversalRenderer renderer)
        {
            foreach (var v in Feature.floatValues)
                if (v.IsValid) cmd.SetGlobalFloat(v.name, v.value);

            foreach (var v in Feature.vectorValues)
                if (v.IsValid) cmd.SetGlobalVector(v.name, v.value);

            foreach (var v in Feature.colorValues)
                if (v.IsValid) cmd.SetGlobalColor(v.name, v.value);

            foreach (var v in Feature.intValues)
                if (v.IsValid) cmd.SetGlobalInt(v.name, v.value);

            foreach (var v in Feature.textureValues)
                if (v.IsValid) cmd.SetGlobalTexture(v.name, v.value);

            foreach (var v in Feature.rtNames)
            {
                if (v.IsValid && !string.IsNullOrEmpty(v.name) && !string.IsNullOrEmpty(v.value))
                {
                    RenderTargetIdentifier rtId = v.value;
                    renderer.FindTarget(v.value, ref rtId);
                    cmd.SetGlobalTexture(v.name, rtId);
                }
            }
        }

        private void SetupCameraLightRotateMatrix(CommandBuffer cmd,Camera cam,Light mainLight)
        {
            if (Feature.isSetMainCameraAndLight)
            {
                if (cam)
                {
                    cmd.SetGlobalMatrix("_CameraRot", Matrix4x4.Rotate(Quaternion.Euler(cam.transform.eulerAngles)));
                    cmd.SetGlobalMatrix("_CameraYRot", Matrix4x4.Rotate(Quaternion.Euler(0, cam.transform.eulerAngles.y, 0)));
                }

                if (mainLight)
                    cmd.SetGlobalMatrix("_MainLightYRot", Matrix4x4.Rotate(Quaternion.Euler(0, mainLight.transform.eulerAngles.y, 0)));
            }
            else
            {
                cmd.SetGlobalMatrix("_CameraRot", Matrix4x4.identity);
                cmd.SetGlobalMatrix("_CameraYRot", Matrix4x4.identity);
                cmd.SetGlobalMatrix("_MainLightYRot", Matrix4x4.identity);
            }
        }

        private void UpdateMinVersion()
        {
            //ShaderEx.SetKeywords(Feature.isMinVersionOn, ShaderKeywords.MIN_VERSION);
            Shader.globalMaximumLOD = Feature.isMinVersionOn ? 100 : 300;
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
            SetupVariables(cmd, ref renderingData);

            UpdateURPAsset();
        }

        private void UpdateURPAsset()
        {
            var urpAsset = UniversalRenderPipeline.asset;
            if (!urpAsset)
                return;

            // save last or reset 
            if (CompareTools.CompareAndSet(ref Feature.lastIsOverrideRenderScale, ref Feature.isOverrideRenderScale))
            {
                if (Feature.isOverrideRenderScale)
                {
                    assetRenderScaleDict[urpAsset] = urpAsset.renderScale;
                }
                else
                    urpAsset.renderScale = assetRenderScaleDict.ContainsKey(urpAsset) ? assetRenderScaleDict[urpAsset] : 1;
            }

            if(Feature.isOverrideRenderScale)
                urpAsset.renderScale = Feature.renderScale;
        }
    }
}
