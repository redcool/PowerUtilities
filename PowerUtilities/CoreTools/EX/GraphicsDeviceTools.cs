using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class GraphicsDeviceTools
    {
        public static bool IsDeviceSupportInstancing()
        {
            // some android es3, dont support DrawMeshInstanced
            var isESDevice = (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                || (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2);
            if (isESDevice)
                return false;

            return SystemInfo.supportsInstancing;
        }
    }
}
