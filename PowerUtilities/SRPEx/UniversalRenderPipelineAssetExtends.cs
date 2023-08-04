using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// UniversalRenderPipelineAsset tools
    /// 
    /*
     * 
        var asset = (UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
        var features = asset.GetRendererDatas().FirstOrDefault().rendererFeatures;
        features.ForEach(f => {
            Debug.Log(f);
        });
    */
    /// </summary>
    public static class UniversalRenderPipelineAssetExtends
    {
        /// <summary>
        /// UniversalRenderPipelineAsset 's private field
        /// </summary>
        static string[] privateFieldNames = new[] {
            "m_RendererDataList",
            "m_Renderers"
        };
        static Dictionary<string, Lazy<FieldInfo>> urpAssetFieldDict = new Dictionary<string, Lazy<FieldInfo>>();
        static UniversalRenderPipelineAssetExtends()
        {
            urpAssetFieldDict.Clear();

            foreach (var fieldName in privateFieldNames)
            {
                urpAssetFieldDict[fieldName] = new Lazy<FieldInfo>(() => typeof(UniversalRenderPipelineAsset).GetField(fieldName, BindingFlags.NonPublic| BindingFlags.Instance));
            }
        }

        /// <summary>
        /// Get UniversalRenderPipelineAsset's private field value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asset"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T[] GetDatas<T>(this UniversalRenderPipelineAsset asset, string fieldName)
        {
            if (!urpAssetFieldDict.ContainsKey(fieldName) || urpAssetFieldDict[fieldName].Value == null)
                return default;

            var datas = (object[])urpAssetFieldDict[fieldName].Value.GetValue(asset);

            if (datas == default)
                return default;

            return datas.Select(data => (T)data).ToArray();
        }

        public static ScriptableRendererData[] GetRendererDatas(this UniversalRenderPipelineAsset asset)
            => GetDatas<ScriptableRendererData>(asset, "m_RendererDataList");

        public static UniversalRenderer[] GetRenderers(this UniversalRenderPipelineAsset asset)
            => GetDatas<UniversalRenderer>(asset, "m_Renderers");

        public static int GetDefaultRendererIndex(this UniversalRenderPipelineAsset asset)
            => asset.GetType().GetFieldValue<int>(asset, "m_DefaultRendererIndex");

        public static UniversalRenderer GetDefaultRenderer(this UniversalRenderPipelineAsset asset)
            => asset.scriptableRenderer as UniversalRenderer;

        /// <summary>
        /// shadowMask or distance shadowMask, call this in OnCameraSetup
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static bool IsShadowMask_Substract(this UniversalRenderPipelineAsset asset, ref RenderingData renderingData,out bool isSubstract)
        {
            var f = asset.GetDefaultRenderer()?.GetForwardLights();
            var mixedLighting = f.GetMixedlightingSetup(ref renderingData);

            isSubstract = asset.supportsMixedLighting && mixedLighting == MixedLightingSetup.Subtractive;
            return asset.supportsMixedLighting && mixedLighting == MixedLightingSetup.ShadowMask;
        }

        public static bool IsLightmapShadowMixing(this UniversalRenderPipelineAsset asset, ref RenderingData renderingData)
        {
            var isShadowMask = IsShadowMask_Substract(asset, ref renderingData, out var isSubstract);
            var isShadowMaskAlways = isShadowMask && QualitySettings.shadowmaskMode == ShadowmaskMode.Shadowmask;
            return isShadowMaskAlways || isSubstract;
        }

    }
}
