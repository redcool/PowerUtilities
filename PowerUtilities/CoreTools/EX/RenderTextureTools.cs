using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Unity.Collections;

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
        /// check rt.size,uav, with desc
        /// </summary>
        /// <param name="rt"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static bool IsNeedAlloc(this RenderTexture rt, RenderTextureDescriptor desc)
        {
            var isSizeInvalid = (!rt || rt.width != desc.width || rt.height != desc.height || rt.depth != desc.depthBufferBits);
            if (isSizeInvalid)
                return true;
            var isDimension = rt.dimension != desc.dimension;
            var isUAV = rt.enableRandomWrite != desc.enableRandomWrite;
            return isUAV || isDimension;
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
            // clone rt info,recreate it
            if(rt && rt.name != name)
            {
                rt = null;
            }
            // camera size changed
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

        public static void AsyncGPUReadRenderTexture(this RenderTexture rt, int pixelByteCount = 4, Action<byte[]> onDone = null)
        {
            if (onDone == null || !rt)
                return;

            var buffer = new NativeArray<byte>(rt.width * rt.height * pixelByteCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            AsyncGPUReadback.RequestIntoNativeArray(ref buffer, rt, 0, request =>
            {
                if (request.hasError)
                {
                    buffer.Dispose();
                    return;
                }
                using var enc = ImageConversion.EncodeNativeArrayToPNG(buffer, rt.graphicsFormat, (uint)rt.width, (uint)rt.height);
                onDone(enc.ToArray());
                buffer.Dispose();
            });
        }

        public static void ReadRenderTexture(this RenderTexture sourceTex, ref Texture2D targetTex)
        {
            if (!sourceTex)
                return;

            if (!targetTex)
                targetTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.ARGB32, false, true);

            RenderTexture.active = sourceTex;
            targetTex.ReadPixels(new Rect(0, 0, sourceTex.width, sourceTex.height), 0, 0, false);
            RenderTexture.active = null;
        }
    }
}
