using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// <summary>
        /// gles 3.1 +
        /// </summary>
        /// <returns></returns>
        public static bool IsGLES3_1Plus()
            => SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3
            || SystemInfo.graphicsDeviceVersion.Contains("OpenGL ES 3.1")
            || SystemInfo.graphicsDeviceVersion.Contains("OpenGL ES 3.2")
        ;

    }
}
