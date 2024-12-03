using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace PowerUtilities
{
    public static class RenderTextureTools
    {

        public static readonly bool m_ForceShadowPointSampling;
        private static readonly RenderTextureFormat m_ShadowmapFormat;

        /// <summary>
        /// CreateRenderTarget's renderTexture
        /// </summary>
        public static readonly Dictionary<string, RenderTexture> createdRenderTextureDict = new();

        static RenderTextureTools()
        {
            m_ForceShadowPointSampling = SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal &&
                GraphicsSettings.HasShaderDefine(Graphics.activeTier, BuiltinShaderDefine.UNITY_METAL_SHADOWS_USE_POINT_FILTERING);

            m_ShadowmapFormat = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap) && (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
            ? RenderTextureFormat.Shadowmap
            : RenderTextureFormat.Depth;
        }

        public static RenderTexture GetTemporaryShadowTexture(int width, int height, int bits)
        {
            //var format = GraphicsFormatUtility.GetDepthStencilFormat(bits, 0);
            //RenderTextureDescriptor rtd = new RenderTextureDescriptor(width, height, GraphicsFormat.None, format);

            RenderTextureDescriptor rtd = new RenderTextureDescriptor(width, height, m_ShadowmapFormat, bits);
            rtd.shadowSamplingMode = (RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Shadowmap)
                && (SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)) ?
                ShadowSamplingMode.CompareDepths : ShadowSamplingMode.None;
            var shadowTexture = RenderTexture.GetTemporary(rtd);
            shadowTexture.filterMode = m_ForceShadowPointSampling ? FilterMode.Point : FilterMode.Bilinear;
            shadowTexture.wrapMode = TextureWrapMode.Clamp;
            return shadowTexture;
        }

        public static bool IsNeedAlloc(this RenderTexture rt, RenderTextureDescriptor desc)
        {
            return (!rt || rt.width != desc.width || rt.height != desc.height || rt.depth != desc.depthBufferBits);
        }

        /// <summary>
        /// create rt and save to dict with name
        /// then can get it with name
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="desc"></param>
        /// <param name="name"></param>
        public static void CreateRT(ref RenderTexture rt,RenderTextureDescriptor desc,string name,FilterMode filterMode)
        {
            if (!rt.IsNeedAlloc(desc))
                return;

            if (rt)
                rt.Release();

            rt = new RenderTexture(desc);
            rt.filterMode = filterMode;
            rt.Create();
            rt.name = name;

            if (!string.IsNullOrEmpty(name))
            {
                if (createdRenderTextureDict.ContainsKey(name))
                    createdRenderTextureDict[name]?.Release();

                createdRenderTextureDict[name] = rt;
            }
        }

        public static bool TryGetRT(string name, out RenderTexture rt)
        => createdRenderTextureDict.TryGetValue(name, out rt);
        
    }
}
