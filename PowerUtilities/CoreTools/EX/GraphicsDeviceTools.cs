using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class GraphicsDeviceTools
    {
        public static bool IsDeviceSupportInstancing() =>
        SystemInfo.supportsInstancing;


        public static bool IsGLDevice()
            => SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2
            ;
    }
}
