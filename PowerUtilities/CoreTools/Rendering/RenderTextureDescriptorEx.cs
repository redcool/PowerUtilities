using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;

namespace PowerUtilities
{
    public static class RenderTextureDescriptorEx
    {
        public static void SetupColorDescriptor(ref this RenderTextureDescriptor desc, Camera camera, float renderScale = 1,bool isHdr=false)
        {
            desc.width = (int)(camera.pixelWidth * renderScale);
            desc.height = (int)(camera.pixelHeight * renderScale);
            desc.msaaSamples = 1;
            desc.dimension = TextureDimension.Tex2D;
            desc.colorFormat = isHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            //desc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt;
        }

        public static void SetupDepthDescriptor(ref this RenderTextureDescriptor desc,Camera camera,float renderScale=1)
        {
            desc.SetupColorDescriptor(camera, renderScale);
            desc.colorFormat = RenderTextureFormat.Depth;
            desc.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D24_UNorm_S8_UInt;
        }
    }
}
