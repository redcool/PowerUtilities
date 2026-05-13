#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// Only in Unity 6000.4 or newer
    /// </summary>
    public static class CameraData_6000
    {
        /// <summary>
        /// SRPPass_6000 inject this. 
        /// </summary>
        public static TextureUVOrigin textureUVOrigin;
        
        public static bool IsCameraProjectionMatrixFlipped(this CameraData data)
        {
            return textureUVOrigin == TextureUVOrigin.BottomLeft;
        }
    }
}
#endif