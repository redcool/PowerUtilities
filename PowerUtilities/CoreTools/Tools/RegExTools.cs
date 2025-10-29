using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class RegExTools
    {
        public static string
            WORD = @"\w+",
            NUMBER = @"\d+",
            GRAPHICS_FORMAT_ASTC =@"\w+ASTC\w+",
            GRAPHICS_FORMAT_ETC = @"\w+(ETC)|(EAC)\w+",
            GRAPHICS_FORMAT_PVRTC=@"\w+PVRTC\w+",
            GRAPHICS_FORMAT_DXT = @"\w+(DXT)|(BC)\w+",
            GRAPHICS_FORMAT_VIDEO = @"(YUV)|(Video)\w+",
            // D16_UNorm,S8_Uint
            GRAPHICS_FORMAT_DEPTH = @"([SD]\d+)|(Depth)|(Shadow)\w+",
            // r8g8b8a8
            GRAPHICS_FORMAT_COLOR_RGBA = @"([rR]\d+)([gG]\d+)?([bB]\d+)?([aA]\d+)?\w+",
            GRAPHICS_FORMAT_COLOR_ARGB = @"([aA]\d+)([bB]\d+)([gG]\d+)([rR]\d+)\w+"
;


        /// <summary>
        /// Get Matched string 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="matchStr"></param>
        /// <returns></returns>
        public static string GetMatch(string str, string matchStr)
            => Regex.Match(str, matchStr)?.Value;

        /// <summary>
        /// str is color format(rgba,argb,abgr)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsRGBAFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_COLOR_ARGB) || Regex.IsMatch(str, GRAPHICS_FORMAT_COLOR_RGBA);
        
        public static bool IsDepthFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_DEPTH);

        public static bool IsVideoFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_VIDEO);

        public static bool IsDXTFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_DXT);

        public static bool IsPVRTCFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_PVRTC);

        public static bool IsETCFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_ETC);
        public static bool IsASTCFormat(string str)
            => Regex.IsMatch(str, GRAPHICS_FORMAT_ASTC);
    }
}
