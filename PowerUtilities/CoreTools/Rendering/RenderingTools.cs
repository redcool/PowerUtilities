using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// srp render path tools
    /// </summary>
    public static class RenderingTools
    {
        /// <summary>
        /// Initial first(
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
        public static void InitEmptyTextures()
        {
            Shader.SetGlobalTexture(ShaderPropertyIds._MainLightShadowmapTexture, RenderingTools.EmptyShadowMap);
            Shader.SetGlobalTexture(ShaderPropertyIds._AdditionalLightsShadowmapTexture, RenderingTools.EmptyShadowMap);
        }



        public static Material ErrorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

        public static void ConvertStringArray<T>(ref T[] results, Func<string, T> onConvert, params string[] names)
        {
            if (onConvert == null || names == null)
                return;

            if(results == null || results.Length != names.Length)
                results = new T[names.Length];

            for(int i = 0; i < names.Length; i++)
            {
                results[i] = onConvert(names[i]);
            }
            //results = names.
            //    Select(n => onConvert(n)).
            //    ToArray();
        }
        /// <summary>
        /// Change names to RenderTargetIdentitier(Shader.PropertyToID
        /// 
        /// name is empty : return defaultId
        /// name is CameraTarget : return BuiltinRenderTextureType.CameraTarget
        /// other : return new RenderTargetIdentifier(n)
        /// </summary>
        /// <param name="names"></param>
        /// <param name="ids"></param>
        /// <param name="defaultId"></param>
        public static void RenderTargetNameToIdentifier(string[] names, ref RenderTargetIdentifier[] ids, BuiltinRenderTextureType defaultId = BuiltinRenderTextureType.CurrentActive)
        => ConvertStringArray(ref ids,(n) => NameToId(n),names);


        public static void RenderTargetNameToInt(string[] names, ref int[] ids)
        => ConvertStringArray(ref ids, (n) => Shader.PropertyToID(n), names);


        public static void ShaderTagNameToId(string[] tagNames, ref ShaderTagId[] ids)
        => ConvertStringArray(ref ids, (n) => new ShaderTagId(n), tagNames);

        public static bool IsNeedCreateTexture(Texture t, int targetWidth, int targetHeight)
            => !(t && t.width == targetWidth && t.height == targetHeight);


        static NativeArray<RenderStateBlock> errorRenderStateBlockArr;

        //[ApplicationExit]
        //[CompileStarted]
        static void DisposeNative()
        {
            if(errorRenderStateBlockArr.IsCreated)
            errorRenderStateBlockArr.Dispose();
        }

        static RenderingTools()
        {
            ApplicationTools.OnDomainUnload += DisposeNative;
        }

        public static void DrawErrorObjects(CommandBuffer cmd,ref ScriptableRenderContext context,ref CullingResults cullingResults,Camera cam,FilteringSettings filterSettings,SortingCriteria sortFlags)
        {
            var sortingSettings = new SortingSettings(cam) { criteria = sortFlags };
            var drawSettings = new DrawingSettings(ShaderTagIdEx.legacyShaderPassNames[0], sortingSettings)
            {
                perObjectData = PerObjectData.None,
                overrideMaterial = ErrorMaterial,
                overrideMaterialPassIndex = 0
            };
            for (int i = 1; i < ShaderTagIdEx.legacyShaderPassNames.Count; i++)
            {
                drawSettings.SetShaderPassName(i, ShaderTagIdEx.legacyShaderPassNames[i]);
            }

#if UNITY_2022_1_OR_NEWER
            NativeArrayTools.CreateIfNull(ref errorRenderStateBlockArr, 1);
            errorRenderStateBlockArr[0] = default;

            context.DrawRenderers(cmd, cullingResults, ref drawSettings, ref filterSettings, null, errorRenderStateBlockArr);
#else
            context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);
#endif

        }

        /// <summary>
        /// Get normal color texture format auto
        /// </summary>
        /// <returns></returns>
        public static GraphicsFormat GetNormalTextureFormat()
        {
            if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R8G8B8A8_SNorm, FormatUsage.Render))
                return GraphicsFormat.R8G8B8A8_SNorm;
            else if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Render))
                return GraphicsFormat.R16G16B16A16_SFloat;

            return GraphicsFormat.R32G32B32A32_SFloat;
        }

        public static RenderTargetIdentifier NameToId(string rtName,BuiltinRenderTextureType defaultId = BuiltinRenderTextureType.CameraTarget)
        {
            if (string.IsNullOrEmpty(rtName))
                return defaultId;

            return rtName == "CameraTarget" ? BuiltinRenderTextureType.CameraTarget : new RenderTargetIdentifier(rtName);
        }

        static RenderTexture emptyShadowMap;
        /// <summary>
        /// Defulat empty shadowmap,
        /// Texture2D.whiteTexture,some device will crash.
        /// 
        /// not clear, _BigShadowParams.x is shadowIntensity,
        /// first time need render bigShadow once, otherwist _BigShadowMap is black
        /// </summary>
        public static RenderTexture GetEmptyShadowMap(ref RTHandle emptyShadowMapHandle)
        {
            if (emptyShadowMap == null)
            {
#if UNITY_2022_1_OR_NEWER
                emptyShadowMapHandle = ShadowUtils.AllocShadowRT(1, 1, 16, 1, 0, "");
                emptyShadowMap = emptyShadowMapHandle.rt;
#else
                emptyShadowMap = RenderTextureTools.GetTemporaryShadowTexture(1, 1, 16);
#endif
            }
            return emptyShadowMap;
        }

        public static RenderTexture EmptyShadowMap
        {
            get
            {
                RTHandle emptyShadowMapHandle = null;
                return GetEmptyShadowMap(ref emptyShadowMapHandle);
            }
        }
    }
}
