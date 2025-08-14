using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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


        /// <summary>
        /// Get (normal texture) format auto,[-1,1]
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

        /// <summary>
        /// Get color format
        /// </summary>
        /// <param name="isHdr"></param>
        /// <param name="hasAlpha"></param>
        /// <returns></returns>
        public static GraphicsFormat GetColorTextureFormat(bool isHdr = false, bool hasAlpha = false)
        {
            if (isHdr)
            {
                if (!hasAlpha && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Linear | FormatUsage.Render))
                    return GraphicsFormat.B10G11R11_UFloatPack32; // mobile hdr
                else if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Linear | FormatUsage.Render))
                    return GraphicsFormat.R16G16B16A16_SFloat;
                else if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R32G32B32A32_SFloat, FormatUsage.Linear | FormatUsage.Render))
                    return GraphicsFormat.R32G32B32A32_SFloat;
                return SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
            }
            // ldr

            if (RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R8G8B8A8_UNorm, FormatUsage.Linear | FormatUsage.Render))
                return GraphicsFormat.R8G8B8A8_UNorm;

            return SystemInfo.GetGraphicsFormat(DefaultFormat.LDR); //GraphicsFormat.R8G8B8A8_SRGB
        }

        public static GraphicsFormat Get(DefaultFormat df)
            => SystemInfo.GetGraphicsFormat(df);

        public static void ShowFormatRelations()
        {
            var sb = new StringBuilder();
            sb.AppendLine("GraphicsFormat      TextureFormat      RenderTextureFormat");
            var ns = Enum.GetValues(typeof(GraphicsFormat));
            const int width = -50;
            foreach (GraphicsFormat gf in ns)
            {
                var tf = GraphicsFormatUtility.GetTextureFormat(gf);
                var rtf = GraphicsFormatUtility.GetRenderTextureFormat(gf);

                sb.AppendLine($"{gf,width}    {tf,width}    {rtf,width}");
            }
            Debug.Log(sb);
        }
    }
}
