namespace PowerUtilities
{
    using System;
    using Unity.Collections;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    /// <summary>
    /// Update Drp's Light & shadow shader variables
    /// </summary>
    public static class DrpLightShaderVarables
    {

        // drp light datas
        public static readonly int _LightColor0;
        public static readonly int _WorldSpaceLightPos0;
        public static readonly int _MainLightShadowmapTexture;
        public static readonly int _ShadowBias;
        public static readonly int _MainLightShadowOn;


        static DrpLightShaderVarables()
        {
            _LightColor0 = Shader.PropertyToID(nameof(_LightColor0));
            _WorldSpaceLightPos0 = Shader.PropertyToID(nameof(_WorldSpaceLightPos0));
            _MainLightShadowmapTexture = Shader.PropertyToID("_MainLightShadowmapTexture");

            _ShadowBias = Shader.PropertyToID("unity_LightShadowBias");
            _MainLightShadowOn = Shader.PropertyToID(nameof(_MainLightShadowOn));

        }

        public static void SendLight(CommandBuffer cmd, RenderingData renderingData)
        {
            var lightData = renderingData.lightData;
            if (lightData.mainLightIndex < 0)
                return;



            // light
            var vLight = lightData.visibleLights[lightData.mainLightIndex];
            cmd.SetGlobalVector(_WorldSpaceLightPos0, -vLight.localToWorldMatrix.GetColumn(2));
            cmd.SetGlobalColor(_LightColor0, vLight.finalColor);

            // shadow bias
            var shadowData = renderingData.shadowData;
            var shadowResolution = ShadowUtils.GetMaxTileResolutionInAtlas(shadowData.mainLightShadowmapWidth, shadowData.mainLightShadowmapHeight, shadowData.mainLightShadowCascadesCount);
            Matrix4x4 viewMat, projMat;
            ShadowSplitData shadowSplitData;
            renderingData.cullResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(lightData.mainLightIndex, 0, shadowData.mainLightShadowCascadesCount, shadowData.mainLightShadowCascadesSplit, shadowResolution, vLight.light.shadowNearPlane, out viewMat, out projMat, out shadowSplitData);

            Vector4 shadowBias = ShadowUtils.GetShadowBias(ref vLight, lightData.mainLightIndex, ref renderingData.shadowData, projMat, shadowResolution);

            cmd.SetGlobalVector(_ShadowBias, shadowBias);

            var asset = UniversalRenderPipeline.asset;
            cmd.SetGlobalFloat(_MainLightShadowOn, asset.supportsMainLightShadows ? 1 : 0);

        }
    }

    /// <summary>
    /// update PowerLit.shader's variables
    /// </summary>
    public static class PowerLitShaderVariables
    {

        //const string MAIN_LIGHT_MODE_ID = "_MainLightMode";
        //const string ADDITIONAL_LIGHT_MODE_ID = "_AdditionalLightMode";

        public static readonly int
            //_MainLightMode = Shader.PropertyToID("_MainLightMode"),
            //_AdditionalLightMode = Shader.PropertyToID("_AdditionalLightMode"),
            //_MainLightShadowCascadeOn = Shader.PropertyToID("_MainLightShadowCascadeOn"),
            //_LightmapOn = Shader.PropertyToID("_LightmapOn"),
            _Shadows_ShadowMaskOn = Shader.PropertyToID("_Shadows_ShadowMaskOn"),
            //_MainLightShadowOn = Shader.PropertyToID("_MainLightShadowOn"),
            //_DistanceShadowMaskOn = Shader.PropertyToID(nameof(_DistanceShadowMaskOn)),
            _LightmapParams = Shader.PropertyToID(nameof(_LightmapParams))
            ;
        public static void SendParams(CommandBuffer cmd, PowerLitFeature.Settings settings,ref RenderingData renderingData)
        {
            var asset = UniversalRenderPipeline.asset;
            var mainLightCastShadows = renderingData.shadowData.supportsMainLightShadows;


            //cmd.SetGlobalInt(_MainLightShadowCascadeOn, asset.shadowCascadeCount > 1 ? 1 : 0);
            //cmd.SetGlobalInt(_LightmapOn, settings._LightmapOn ? 1 : 0);
            cmd.SetGlobalInt(_Shadows_ShadowMaskOn, settings._Shadows_ShadowMaskOn ? 1 : 0);
            //cmd.SetGlobalInt(_MainLightShadowOn, mainLightCastShadows ? 1 : 0);
            //cmd.SetGlobalInt(_MainLightMode, (int)asset.mainLightRenderingMode);
            //cmd.SetGlobalInt(_AdditionalLightMode, (int)asset.additionalLightsRenderingMode);

            //cmd.SetGlobalFloat(_DistanceShadowMaskOn, QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask ? 1 : 0);
            cmd.SetGlobalVector(_LightmapParams, new Vector4(settings._LightmapSH,
                settings._LightmapSaturate,
                settings._LightmapIntensity,
                0));
        }
    }


    public class PowerLitFeature : ScriptableRendererFeature
    {

        [Serializable]
        public class Settings
        {
            //public bool isActive;
            //[Header("Main Light Shadow")]
            //[NonSerialized] public bool _MainLightShadowOn;
            //[NonSerialized] public bool _MainLightShadowCascadeOn;
            //[NonSerialized] public bool _AdditionalVertexLightOn;

            [Tooltip("enable shadowMask ?")] public bool _Shadows_ShadowMaskOn;

            [Header("GI")]
            //[Tooltip("enabled lightmap ?")] public bool _LightmapOn;
            [Tooltip("blend Lightmap SH")][Range(0,1)] public float _LightmapSH=0.5f;
            [Tooltip("strength lightmap saturate")][Range(0,1)] public float _LightmapSaturate=1;
            [Tooltip("strength lightmap intensity")] [Range(1,4)]public float _LightmapIntensity=1;
            
            [Header("Drp Adpater")]
            public bool updateDRPShaderVarables;

        }

        class PowerLitUpdateParamsPass : ScriptableRenderPass
        {
            public Settings settings;

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                PowerLitShaderVariables.SendParams(cmd,settings,ref renderingData);
                if (settings.updateDRPShaderVarables)
                {
                    DrpLightShaderVarables.SendLight(cmd, renderingData);
                }
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {

            }
        }
        public Settings settings = new Settings();
        PowerLitUpdateParamsPass pass;


        /// <inheritdoc/>
        public override void Create()
        {
            pass = new PowerLitUpdateParamsPass();
            pass.settings = settings;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(pass);
        }
    }


}