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
        /// <summary>
        /// check rt.size with desc
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static bool IsNeedAlloc(this RenderTexture rt, RenderTextureDescriptor desc)
        {
            return IsNeedAlloc(rt, desc.width, desc.height, desc.depthBufferBits);
        }
        public static bool IsNeedAlloc(this RenderTexture rt, int width,int height,int depth)
        {
            return (!rt || rt.width != width || rt.height != height || rt.depth != depth);
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
            {
                AddRT(rt, name);
                return;
            }

            if (rt)
            {
                DestroyRT(name);
            }
            rt = new RenderTexture(desc);
            rt.filterMode = filterMode;
            rt.Create();
            rt.name = name;

            AddRT(rt, name);
        }

        /// <summary>
        /// create rt when rt' size diff with desc
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="desc"></param>
        /// <param name="rtName"></param>
        /// <param name="filterMode"></param>
        public static void TryCreateRT(ref RenderTexture rt, RenderTextureDescriptor desc, string rtName, FilterMode filterMode)
        {
            if (IsNeedAlloc(rt, desc))
            {
                if (rt)
                    rt.Destroy();

                CreateRT(ref rt, desc, rtName, filterMode);
            }
        }

        /// <summary>
        /// save rt to dict, TryGetRT get it
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="name"></param>
        public static void AddRT(RenderTexture rt, string name)
        {
            if (string.IsNullOrEmpty(name) || !rt)
                return;
            createdRenderTextureDict[name] = rt;
            // set global texture
            Shader.SetGlobalTexture(name, rt);
        }

        /// <summary>
        /// Get CreateRenderTarget's rt
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rt"></param>
        /// <returns></returns>
        public static bool TryGetRT(string name, out RenderTexture rt)
        => createdRenderTextureDict.TryGetValue(name, out rt);
        
        /// <summary>
        /// remove CreateRenderTarget's rt
        /// </summary>
        /// <param name="rt"></param>
        public static void DestroyRT(RenderTexture rt)
        {
            if (!rt)
                return;

            createdRenderTextureDict.Remove(rt.name);
            rt.Destroy();
        }

        public static void DestroyRT(string name)
        {
            if (string.IsNullOrEmpty(name) || !createdRenderTextureDict.ContainsKey(name))
                return;

            createdRenderTextureDict[name].Destroy();
            createdRenderTextureDict.Remove(name);
        }
    }
}
