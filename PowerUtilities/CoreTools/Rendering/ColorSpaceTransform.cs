using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// Change colorSpace 
    /// </summary>
    public static class ColorSpaceTransform
    {
        public enum ColorSpaceMode
        {
            None, LinearToSRGB, SRGBToLinear
        }

        public static void SetColorSpace(CommandBuffer cmd, ColorSpaceMode trans)
        {
            switch (trans)
            {
                case ColorSpaceMode.LinearToSRGB:
                    cmd.EnableShaderKeyword(ShaderPropertyIds._LINEAR_TO_SRGB_CONVERSION);
                    cmd.DisableShaderKeyword(ShaderPropertyIds._SRGB_TO_LINEAR_CONVERSION);
                    break;

                case ColorSpaceMode.SRGBToLinear:
                    cmd.EnableShaderKeyword(ShaderPropertyIds._SRGB_TO_LINEAR_CONVERSION);
                    cmd.DisableShaderKeyword(ShaderPropertyIds._LINEAR_TO_SRGB_CONVERSION);
                    break;

                default:
                    cmd.DisableShaderKeyword(ShaderPropertyIds._SRGB_TO_LINEAR_CONVERSION);
                    cmd.DisableShaderKeyword(ShaderPropertyIds._LINEAR_TO_SRGB_CONVERSION);
                    break;
            }
        }


    }
}
