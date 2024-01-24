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
        public static List<ShaderTagId> legacyShaderPassNames = new List<ShaderTagId>
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        public static List<ShaderTagId> urpForwardShaderPassNames= new List<ShaderTagId>{
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("LightweightForward")
        };
        public static List<ShaderTagId> shadowCaster = new List<ShaderTagId>
        {
            new ShaderTagId("ShadowCaster")
        };
        public static List<ShaderTagId> depthOnly = new List<ShaderTagId>
        {
            new ShaderTagId("DepthOnly")
        };

        public static Material ErrorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));

        public static void ConvertStringArray<T>(ref T[] results, Func<string, T> onConvert, params string[] names)
        {
            if (onConvert == null || names == null)
                return;

            results = names.
                Select(n => onConvert(n)).
                ToArray();
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

        [ApplicationExit]
        [CompileStarted]
        static void DisposeNative()
        {
            if(errorRenderStateBlockArr.IsCreated)
            errorRenderStateBlockArr.Dispose();
        }

        public static void DrawErrorObjects(CommandBuffer cmd,ref ScriptableRenderContext context,ref CullingResults cullingResults,Camera cam,FilteringSettings filterSettings,SortingCriteria sortFlags)
        {
            var sortingSettings = new SortingSettings(cam) { criteria = sortFlags };
            var drawSettings = new DrawingSettings(legacyShaderPassNames[0], sortingSettings)
            {
                perObjectData = PerObjectData.None,
                overrideMaterial = ErrorMaterial,
                overrideMaterialPassIndex = 0
            };
            for (int i = 1; i < legacyShaderPassNames.Count; i++)
            {
                drawSettings.SetShaderPassName(i, legacyShaderPassNames[i]);
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
    }
}
