using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public enum PresetBlendMode
    {
        Normal,
        AlphaBlend,
        SoftAdd,
        Add,
        PremultiTransparent,
        MultiColor,
        MultiColor_2X
    }
    /// <summary>
    /// PresetBlendMode control shader's srcMode and dstMode
    /// 
    /// use in PowerShaderInspector and GroupAPI(GroupPresetBlendMode)
    /// </summary>
    public static class PresetBlendModeTools
    {
        public static Dictionary<PresetBlendMode, BlendMode[]> blendModeDict = new Dictionary<PresetBlendMode, BlendMode[]>
        {
            {PresetBlendMode.Normal,new []{ BlendMode.One,BlendMode.Zero} },
            {PresetBlendMode.AlphaBlend,new []{ BlendMode.SrcAlpha,BlendMode.OneMinusSrcAlpha} },
            {PresetBlendMode.SoftAdd,new []{ BlendMode.SrcAlpha, BlendMode.One} }, //OneMinusDstColor
            {PresetBlendMode.Add,new []{ BlendMode.One,BlendMode.One} },
            {PresetBlendMode.PremultiTransparent,new []{BlendMode.One,BlendMode.OneMinusSrcAlpha } },
            {PresetBlendMode.MultiColor,new []{ BlendMode.DstColor,BlendMode.Zero} },
            {PresetBlendMode.MultiColor_2X,new []{ BlendMode.DstColor,BlendMode.SrcColor} },
        };
        // properties
        public const string SRC_MODE = "_SrcMode", DST_MODE = "_DstMode";

        public static PresetBlendMode GetPresetBlendMode(BlendMode srcMode, BlendMode dstMode)
        {
            return blendModeDict.
                Where(kv => kv.Value[0] == srcMode && kv.Value[1] == dstMode)
                .FirstOrDefault().Key;
        }
        /// <summary>
        /// Get presetBlendMode by material's _SrcMode and _DstMode
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="srcModeName"></param>
        /// <param name="dstModeName"></param>
        /// <returns></returns>
        public static PresetBlendMode GetPresetBlendMode(this Material mat, string srcModeName = SRC_MODE, string dstModeName = DST_MODE)
        {
            var srcMode = mat.GetInt(srcModeName);
            var dstMode = mat.GetInt(dstModeName);
            return GetPresetBlendMode((BlendMode)srcMode, (BlendMode)dstMode);
        }
        /// <summary>
        /// return srcMode & dstMode
        /// </summary>
        /// <param name="presetBlendMode"></param>
        /// <returns></returns>
        public static BlendMode[] GetBlendMode(PresetBlendMode presetBlendMode)
        {
            return blendModeDict[presetBlendMode];
        }

        /// <summary>
        /// Apply presetBlendMode to material
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="blendMode"></param>
        /// <param name="srcModeName"></param>
        /// <param name="dstModeName"></param>
        public static void SetPresetBlendMode(this Material mat, PresetBlendMode blendMode, string srcModeName = SRC_MODE, string dstModeName = DST_MODE)
        {
            if (!mat)
                return;
            if (blendModeDict.TryGetValue(blendMode,out var blendPair))
            {
                mat.SetInt(srcModeName, (int)blendPair[0]);
                mat.SetInt(dstModeName, (int)blendPair[1]);
            }
        }
    }
}
