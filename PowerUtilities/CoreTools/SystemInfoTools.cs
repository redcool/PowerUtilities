using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// Extends SystemInfo 
    /// </summary>
    public static class SystemInfoTools
    {
        static readonly Regex REG_GLES3_1_PLUS = new Regex(@"OpenGL ES (\d+.\d+)");
        /// <summary>
        /// is gles 3.1
        /// </summary>
        /// <returns></returns>
        public static bool IsGLES3_1()
            => SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3
            || SystemInfo.graphicsDeviceVersion.Contains("OpenGL ES 3.1")
        ;
        /// <summary>
        /// is gles 3.2
        /// </summary>
        /// <returns></returns>
        public static bool IsGLES3_2()
            => SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3
            || SystemInfo.graphicsDeviceVersion.Contains("OpenGL ES 3.2")
        ;
        public static float GetGLESVersion()
        {
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
                return 0;
            var result = REG_GLES3_1_PLUS.Match(SystemInfo.graphicsDeviceVersion);
            if (result.Success && result.Groups.Count > 1)
            {
                var versionStr = result.Groups[1].Value;
                if (float.TryParse(versionStr, out var versionNum))
                {
                    return versionNum;
                }
            }
            return 0;
        }

        /// <summary>
        /// gles 3.1 +
        /// </summary>
        /// <returns></returns>
        public static bool IsGLES3_1Plus()
        {
            if(SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
                return false;

            return GetGLESVersion() >= 3.1f;
        }

    }
}
