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
using System.IO;

namespace PowerUtilities
{
    public static class RenderTextureTools
    {

        public static readonly bool m_ForceShadowPointSampling;
        private static readonly RenderTextureFormat m_ShadowmapFormat;

        /// <summary>
        /// {rtName,rt}
        /// CreateRenderTarget's renderTexture
        /// </summary>
        public static readonly Dictionary<string, RenderTexture> nameRTDict = new();

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
            // rt is changed,recreate it
            //if(rt && rt.name != name) // * rt.name will cause gc
            if (rt && nameRTDict.TryGetValue(name, out var existRT) && rt != existRT)
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
            nameRTDict[name] = rt;
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
        => nameRTDict.TryGetValue(name, out rt);
        
        /// <summary>
        /// remove CreateRenderTarget's rt
        /// </summary>
        /// <param name="rt"></param>
        public static void DestroyRT(RenderTexture rt)
        {
            if (!rt)
                return;

            nameRTDict.Remove(rt.name);
            rt.Destroy();
        }

        public static void DestroyRT(string name)
        {
            if (string.IsNullOrEmpty(name) || !nameRTDict.ContainsKey(name))
                return;

            nameRTDict[name].Destroy();
            nameRTDict.Remove(name);
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

        public static void ReadRenderTexture(this RenderTexture sourceTex, ref Texture2D targetTex,bool reCalcMipMaps=true)
        {
            if (!sourceTex)
                return;

            if (!targetTex)
            {
                targetTex = new Texture2D(sourceTex.width, sourceTex.height, TextureFormat.RGBAHalf, false, true);
            }

            RenderTexture.active = sourceTex;
            targetTex.ReadPixels(new Rect(0, 0, sourceTex.width, sourceTex.height), 0, 0,reCalcMipMaps);
            RenderTexture.active = null;
        }
        /// <summary>
        /// get temporary Unordered Access Views
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static RenderTexture GetTemporaryUAV(int width,int height,RenderTextureFormat format,TextureDimension dimension = TextureDimension.Tex2D)
        {
            var desc = new RenderTextureDescriptor(width, height, format);
            desc.dimension = dimension;
            desc.enableRandomWrite = true;
            return RenderTexture.GetTemporary(desc);
        }

        public static void ReleaseSafe(this RenderTexture rt)
        {
            if (rt)
                RenderTexture.ReleaseTemporary(rt);
            rt?.Release();
        }


        public static void SaveRenderTexture(this RenderTexture rt, TextureEncodeType texEncodeType, string assetFolder, string fileNameNoExtName, bool isSRGB)
        {
            PathTools.CreateAbsFolderPath(assetFolder);

            Texture2D tex = null;
            rt.ReadRenderTexture(ref tex, true);

            //var isHDR = GraphicsFormatUtility.IsHDRFormat(rt.graphicsFormat);
            //if (isHDR && texEncodeType != TextureEncodeType.EXR)

            if (isSRGB)
            {
                var colors = rt.GetPixelsConvertColorSpace();
                tex.SetPixels(colors);
                tex.Apply();
            }

            //tex.Compress(true, TextureFormat.ASTC_HDR_6x6);

            var extName = texEncodeType.ToString();
            var filePath = $"{assetFolder}/{fileNameNoExtName}.{extName}";
            File.WriteAllBytes(filePath, tex.GetEncodeBytes(texEncodeType));
            tex.Destroy();
        }
    }
}
