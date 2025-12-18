using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public enum TextureEncodeType
    {
        PNG, TGA, EXR, JPG
    }

    public static class TextureTools
    {
        /// <summary>
        /// Split Textures by resolution
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="resolution"></param>
        /// <param name="countInRow"></param>
        /// <returns></returns>
        public static List<Texture2D> SplitTextures(Texture2D[] textures, TextureResolution resolution, ref int countInRow, Action<float> onProgress, bool isHeightmap, TextureFormat textureFormat = TextureFormat.R8, bool isMipChain = false, bool isLinear = true)
        {
            if (textures == null || textures.Length == 0)
                return null;

            var res = (int)resolution;
            var textureList = new List<Texture2D>();
            countInRow = 0;

            for (int i = 0; i < textures.Length; i++)
            {
                var tex = textures[i];
                if (tex == null)
                    continue;


                var countInRowTile = tex.width / res;
                countInRowTile = Mathf.Max(1, countInRowTile);

                countInRow += countInRowTile;

                if (tex.width > res)
                {
                    var texs = tex.SplitTexture(res, onProgress, isHeightmap, textureFormat, isMipChain, isLinear);
                    textureList.AddRange(texs);
                }
                else
                {
                    textureList.Add(tex);
                }
            }
            return textureList;
        }

        public static Texture2DArray Create2DArray(List<Texture2D> textures, int width, int height, TextureFormat tf,int mipCount, bool linear)
        {
            var arr = new Texture2DArray(width, height, textures.Count, tf, mipCount, linear,true);
            arr.Fill(textures);
            return arr;
        }

        public static Texture2DArray Create2DArray(List<Texture2D> textures, bool linear)
        {
            var q = textures.Where(t => t).ToList();
            if (q.Count == 0)
                return null;

            var sample = q[0];
            return Create2DArray(q, sample.width, sample.height, sample.format, sample.mipmapCount, linear);
        }

        public static Texture3D Create3D(List<Texture2D> textures, bool linear)
        {
            var q = textures.Where(t => t).ToList();
            if (q.Count == 0)
                return null;

            var sample = q[0];
            var tex3D = new Texture3D(sample.width, sample.height, q.Count, sample.format, sample.mipmapCount);
            q.ForEach((tex, index) =>
            {
                Graphics.CopyTexture(tex, 0, 0, tex3D, index, 0);
            });
            return tex3D;
        }

        public static byte[] GetEncodeBytes(this Texture2D tex, TextureEncodeType texType,Texture2D.EXRFlags exrFlags = (Texture2D.EXRFlags)(3)) => texType switch
        {
            TextureEncodeType.TGA => tex.EncodeToTGA(),
            TextureEncodeType.EXR => tex.EncodeToEXR(exrFlags),
            TextureEncodeType.JPG => tex.EncodeToJPG(),
            _ => tex.EncodeToPNG(),
        };

        public static void Compress(this Texture2D tex,bool isHighQuality,TextureFormat tf = TextureFormat.ASTC_6x6)
        {
#if UNITY_EDITOR
            EditorUtility.CompressTexture(tex, tf, isHighQuality ? 100 : 50);
#else
            tex.Compress(isHighQuality);
#endif
        }
        /// <summary>
        /// Convert to srgb or linear
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="coefficient">to srgb: 2 , to linear : 1/2</param>
        public static void ConvertColorSpace(this Texture2D tex,float coefficient = 1/2.2f)
        {
            var pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float4 c = (Vector4)pixels[i];
                c.xyz = math.pow(c.xyz, coefficient);
                pixels[i] = (Vector4)c;
            }
            tex.SetPixels(pixels);
        }

        /// <summary>
        /// Call compute shader to convert color space(srgb<->linear) ,and apply GT6 tone mapping
        /// 
        /// when cs is null, find Resources/Shaders/ColorConvert.compute 
        /// final cs is null,use cpu convert
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="cs"></param>
        /// <param name="kernelName"></param>
        /// <param name="coefficient"></param>
        /// <param name="isApplyGT6Tone"></param>
        public static void ConvertColorSpace(this Texture2D tex, ComputeShader cs, string kernelName= "ConvertColorSpace", float coefficient = 1 / 2.2f, bool isApplyGT6Tone = true)
        {
            //1  find ConvertColorSpace kernel
            if (!cs) 
            {
                cs = ComputeShaderEx.GetCS(ComputeShaderEx.CS_COLOR_CONVERT);
            }
            //2  cs not found, use cpu convert
            if (!cs)
            {
                ConvertColorSpace(tex, coefficient);
                return;
            }

            var pixels = tex.GetPixels();

            GraphicsBuffer colorBuffer = null;
            GraphicsBufferTools.TryCreateBuffer(ref colorBuffer, GraphicsBuffer.Target.Structured, pixels.Length, Marshal.SizeOf<Vector4>());
            colorBuffer.SetData(pixels);

            var kernel = cs.FindKernel(kernelName);
            cs.SetFloat("_Coefficient", coefficient);
            cs.SetBuffer(kernel, "_ColorBuffer", colorBuffer);
            cs.SetBool("_ApplyGT6Tone", isApplyGT6Tone);

            cs.DispatchKernel(kernel, tex.width, tex.height, 1);

            colorBuffer.GetData(pixels);
            colorBuffer.TryRelease();

            tex.SetPixels(pixels);
        }

        public static int GetDepth(this Texture tex)
        {
            if (tex == null)
                return 0;
            if (tex is Texture2DArray texArr)
                return texArr.depth;
            else if (tex is Texture3D tex3D)
                return tex3D.depth;
            else if (tex is RenderTexture rt)
                return rt.volumeDepth;

            return 1;
        }
    }
}
