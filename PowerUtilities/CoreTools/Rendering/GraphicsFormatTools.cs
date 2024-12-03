using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Experimental.Rendering;

namespace PowerUtilities
{
    public static class GraphicsFormatTools
    {
        static Dictionary<GraphicsFormat, string> dict = new();
        /// <summary>
        /// Get Format name string with cache
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetFormatName(GraphicsFormat format)
        => DictionaryTools.Get(dict, format, (gf) => gf.ToString());
        /// <summary>
        /// ColroFormat
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool IsRGBAFormat(this GraphicsFormat format)
        => RegExTools.IsRGBAFormat(GetFormatName(format));

        public static bool IsDepthFormat(this GraphicsFormat format)
        => RegExTools.IsDepthFormat(GetFormatName(format));

        public static bool IsVideoFormat(this GraphicsFormat format)
        => RegExTools.IsVideoFormat(GetFormatName(format));

        public static bool IsDXTFormat(this GraphicsFormat format)
        => RegExTools.IsDXTFormat(GetFormatName(format));

        public static bool IsPVRTCFormat(this GraphicsFormat format)
        => RegExTools.IsPVRTCFormat(GetFormatName(format));

        public static bool IsETCFormat(this GraphicsFormat format)
        => RegExTools.IsETCFormat(GetFormatName(format));
        public static bool IsASTCFormat(this GraphicsFormat format)
        => RegExTools.IsASTCFormat(GetFormatName(format));

        public static bool IsRGBAFormat(string format)
        => RegExTools.IsRGBAFormat(format);
        public static bool IsDepthFormat(string format)
        => RegExTools.IsDepthFormat(format);

        public static bool IsVideoFormat(string format)
        => RegExTools.IsVideoFormat(format);

        public static bool IsDXTFormat(string format)
        => RegExTools.IsDXTFormat(format);

        public static bool IsPVRTCFormat(string format)
        => RegExTools.IsPVRTCFormat(format);

        public static bool IsETCFormat(string format)
        => RegExTools.IsETCFormat(format);

        public static bool IsASTCFormat(string format)
        => RegExTools.IsASTCFormat(format);
    }
}
