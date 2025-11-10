using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PowerUtilities
{
    public enum TextureResolution
    {
        x32 = 32, x64 = 64, x128 = 128, x256 = 256, x512 = 512, x1024 = 1024, x2048 = 2048, x4096 = 4096, x8192 = x4096 * 2, x16384 = x8192 * 2
    }

    public enum DepthBufferBits
    {
        _0 = 0, _16 = 16, _24 = 24, _32 = 32
    }

    public static class Texture2DEx
    {
        static Material fillTextureMat;

        public static Material GetFillTextureMaterial()
        {
            if (!fillTextureMat)
                fillTextureMat = new Material(Shader.Find("Hidden/FillTexture"));
            return fillTextureMat;
        }
#if UNITY_EDITOR
        static List<TextureImporterFormat> uncompressionFormats = new List<TextureImporterFormat>(new[]{
            TextureImporterFormat.ARGB32,
            TextureImporterFormat.RGB24,
            TextureImporterFormat.RGBA32,
            TextureImporterFormat.RGBAHalf,
        });

        public static TextureImporter GetTextureImporter(this Texture tex)
        {
            var path = AssetDatabase.GetAssetPath(tex);
            return AssetImporter.GetAtPath(path) as TextureImporter;
        }

        public static void Setting(this Texture tex, Action<TextureImporter> onSetup)
        {
            if (onSetup == null)
                return;

            onSetup(tex.GetTextureImporter());
        }

        public static bool IsCompressionFormat(this Texture tex, string platform)
        {
            var imp = tex.GetTextureImporter();
            var settings = imp.GetPlatformTextureSettings(platform);
            return !uncompressionFormats.Contains(settings.format);
        }

        public static void SetReadable(this Texture tex, bool isReadable)
        {
            if (tex.isReadable == isReadable)
                return;

            var textureImporter = tex.GetTextureImporter();
            textureImporter.isReadable = isReadable;
            textureImporter.SaveAndReimport();
        }
#endif

        /// <summary>
        /// Clone new tex in memory, it can read/write
        /// </summary>
        /// <param name="tex"></param>
        /// <returns></returns>
        public static Texture2D Clone(this Texture tex)
        {
            Texture2D newTex = null;
            var rt = RenderTexture.GetTemporary(tex.width, tex.height);
            Graphics.Blit(tex, rt);
            rt.ReadRenderTexture(ref newTex);
            RenderTexture.ReleaseTemporary(rt);
            return newTex;
        }
        /// <summary>
        /// Transform (x,y) to uv[0,1],
        /// Samples the color from the specified texture at the given pixel coordinates and mip level.
        /// </summary>
        /// <param name="tex">The texture from which to sample the color. Cannot be <see langword="null"/>.</param>
        /// <param name="x">The horizontal pixel coordinate. Must be within the range [0, <c>tex.width - 1</c>].</param>
        /// <param name="y">The vertical pixel coordinate. Must be within the range [0, <c>tex.height - 1</c>].</param>
        /// <param name="width">when less 0 use texture width</param>
        /// <param name="height">when less 0 use texture height</param>
        /// <param name="mipLevel">The mip level to sample from. Defaults to 0.</param>
        /// <returns>The color sampled from the texture at the specified coordinates and mip level.</returns>
        public static Color Sample(this Texture2D tex, int x, int y, int width = -1, int height = -1, int mipLevel = 0)
        {
            var u = (float)x / (width < 1 ? tex.width - 1 : width - 1);
            var v = (float)y / (height < 1 ? tex.height - 1 : height - 1);
            return tex.GetPixelBilinear(u, v, mipLevel);
        }

        /// <summary>
        /// split texture to textures
        /// texture need power of 2 , width == height
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static Texture2D[] SplitTexture(this Texture2D tex, int resolution, Action<float> onProgress, bool isHeightmap,TextureFormat tileTextureFormat = TextureFormat.RGBA32,bool isMipChain=false,bool isLinear=true)
        {
            if (tex.width <= resolution)
            {
                return new[] { tex };
            }
#if UNITY_EDITOR
            tex.SetReadable(true);
#endif

            var texWidth = resolution + (isHeightmap ? 1 : 0);
            var texHeight = resolution + (isHeightmap ? 1 : 0);
            // splite texture
            var count = tex.width / resolution;
            var newTexs = new Texture2D[count * count];
            var texId = 0;

            for (int y = 0; y < count; y++)
            {
                for (int x = 0; x < count; x++)
                {
                    var blockWidth = resolution + (isHeightmap && x < count - 1 ? 1 : 0);
                    var blockHeight = resolution + (isHeightmap && y < count - 1 ? 1 : 0);

                    var newTex = newTexs[texId++] = new Texture2D(texWidth, texHeight,tileTextureFormat, isMipChain, isLinear);
                    newTex.Fill(Color.black);


                    newTex.SetPixels(0, 0, blockWidth, blockHeight, tex.GetPixels(x * resolution, y * resolution, blockWidth, blockHeight));
                    newTex.Apply();

                    if (onProgress != null)
                        onProgress((float)texId / newTexs.Length);
                }
            }
            return newTexs;
        }

        public static void Fill(this Texture2D tex, Color c)
        {
            var colors = Enumerable.Repeat(c, tex.width * tex.height).ToArray();
            tex.SetPixels(colors);
            //for (int y = 0; y < tex.height; y++)
            //{
            //    for (int x = 0; x < tex.width; x++)
            //    {
            //        tex.SetPixel(x, y, c);
            //    }
            //}
        }
        public static void FillRow(this Texture2D tex,Color c,int rowId)
        {
            var colors = Enumerable.Repeat(c, tex.width).ToArray();
            tex.SetPixels(0, rowId, tex.width, 1, colors);
        }

        public static void FillColumn(this Texture2D tex,Color c,int columnId)
        {
            var colors = Enumerable.Repeat(c, tex.height).ToArray();
            tex.SetPixels(columnId, 0, 1, tex.height, colors);
        }

        /// <summary>
        /// blit from src , write to (destX,destY),(blockWidth,blockHeight)
        /// </summary>
        /// <param name="tex"></param>
        /// <param name="src"></param>
        /// <param name="destX"></param>
        /// <param name="destY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        public static void BlitFrom(this Texture2D tex,Texture src,int destX=0,int destY=0,int blockWidth=-1,int blockHeight=-1, Material mat = null)
        {
            GraphicsEx.Blit(src, tex, mat, destX, destY, blockWidth, blockHeight);
        }

        
    }
}
