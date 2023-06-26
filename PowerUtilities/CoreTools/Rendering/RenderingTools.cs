using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
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

        public static Material ErrorMaterial => MaterialCacheTool.GetMaterial("Hidden/InternalErrorShader");

        public static void ConvertStringArray<T>(ref T[] results, Func<string, T> onConvert, params string[] names)
        {
            if (onConvert == null || names == null)
                return;

            results = names.
                Select(n => onConvert(n)).
                ToArray();
        }

        public static void RenderTargetNameToIdentifier(string[] names, ref RenderTargetIdentifier[] ids, BuiltinRenderTextureType defaultId = BuiltinRenderTextureType.CurrentActive)
        => ConvertStringArray(ref ids,
            (n) => string.IsNullOrEmpty(n)? defaultId : new RenderTargetIdentifier(n),
            names);


        public static void RenderTargetNameToInt(string[] names, ref int[] ids)
        => ConvertStringArray(ref ids, (n) => Shader.PropertyToID(n), names);


        public static void ShaderTagNameToId(string[] tagNames, ref ShaderTagId[] ids)
        => ConvertStringArray(ref ids, (n) => new ShaderTagId(n), tagNames);

        public static bool IsNeedCreateTexture(Texture t, int targetWidth, int targetHeight)
            => !(t && t.width == targetWidth && t.height == targetHeight);

        public static void DrawErrorObjects(ref ScriptableRenderContext context,ref CullingResults cullingResults,Camera cam,FilteringSettings filterSettings,SortingCriteria sortFlags)
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

            context.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);

        }
    }
}
