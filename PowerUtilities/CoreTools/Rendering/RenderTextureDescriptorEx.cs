using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;

namespace PowerUtilities
{
    public static class RenderTextureDescriptorEx
    {
        public static void SetupColorDescriptor(ref this RenderTextureDescriptor desc, int pixelWidth, int pixelHeight, float renderScale = 1, bool isHdr = false, int samples = 1)
        {
            desc.width = (int)(pixelWidth * renderScale);
            desc.height = (int)(pixelHeight * renderScale);
            desc.msaaSamples = samples;
            desc.dimension = TextureDimension.Tex2D;
            desc.colorFormat = isHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            //desc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt;
        }
        /// <summary>
        /// unity editor renderScale need 1
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="camera"></param>
        /// <param name="renderScale"></param>
        /// <param name="isHdr"></param>
        public static void SetupColorDescriptor(ref this RenderTextureDescriptor desc, Camera camera, float renderScale = 1,bool isHdr=false,int samples=1)
        {
            SetupColorDescriptor(ref desc, camera.pixelWidth, camera.pixelHeight, renderScale, isHdr, samples);
        }

        public static void SetupDepthDescriptor(ref this RenderTextureDescriptor desc,Camera camera,float renderScale=1)
        {
            desc.SetupColorDescriptor(camera, renderScale);
            desc.colorFormat = RenderTextureFormat.Depth;
#if UNITY_2020
            desc.depthBufferBits = 24;
#else
            desc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt;
#endif
        }
    }
}
