using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PowerUtilities
{
    public static class TerrainDataEx
    {
        /// <summary>
        /// Update terrainData's heightmap
        /// </summary>
        /// <param name="td"></param>
        /// <param name="heightmap"></param>
        /// <param name="isUpdateHeightmapResolution"></param>
        public static void ApplyHeightmap(this TerrainData td, Texture2D heightmap,bool isUpdateHeightmapResolution=false)
        {
            if (!heightmap)
                return;

            if(isUpdateHeightmapResolution)
                td.heightmapResolution = heightmap.width;

            td.BlitToHeightmap(heightmap);
        }

        static void SetHeights(this TerrainData td,Texture2D heightmap)
        {
            if (!heightmap)
                return;

            var w = heightmap.width;// (int)terrainSize.x;
            var heights = new float[w, w];
            var colors = heightmap.GetPixels();

            for (int y = 0; y < w; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    heights[y, x] = Mathf.GammaToLinearSpace(colors[x + y * w].r);
                }
            }
            td.SetHeights(0, 0, heights);
        }

        public static Material GetBlitHeightmapMaterial()
        {
            //return new Material(Shader.Find("Hidden/TerrainTools/HeightBlit"));
            return new Material(Shader.Find("Hidden/Terrain/BlitTextureToHeightmap"));
        }

        /// <summary>
        /// Change heightmap resolution
        /// </summary>
        /// <param name="terrainData"></param>
        /// <param name="resolution"></param>
        public static void ResizeHeightmap(this TerrainData terrainData, int resolution)
        {
            RenderTexture oldRT = RenderTexture.active;

            RenderTexture oldHeightmap = RenderTexture.GetTemporary(terrainData.heightmapTexture.descriptor);
            Graphics.Blit(terrainData.heightmapTexture, oldHeightmap);
#if UNITY_2019_3_OR_NEWER
            // terrain holes
            RenderTexture oldHoles = RenderTexture.GetTemporary(terrainData.holesTexture.width, terrainData.holesTexture.height);
            Graphics.Blit(terrainData.holesTexture, oldHoles);
#endif

            float sUV = 1.0f;
            int dWidth = terrainData.heightmapResolution;
            int sWidth = resolution;

            Vector3 oldSize = terrainData.size;
            terrainData.heightmapResolution = resolution;
            terrainData.size = oldSize;

            oldHeightmap.filterMode = FilterMode.Bilinear;

            // Make sure textures are offset correctly when resampling
            // tsuv = (suv * swidth - 0.5) / (swidth - 1)
            // duv = (tsuv(dwidth - 1) + 0.5) / dwidth
            // duv = (((suv * swidth - 0.5) / (swidth - 1)) * (dwidth - 1) + 0.5) / dwidth
            // k = (dwidth - 1) / (swidth - 1) / dwidth
            // duv = suv * (swidth * k)		+ 0.5 / dwidth - 0.5 * k

            float k = (dWidth - 1.0f) / (sWidth - 1.0f) / dWidth;
            float scaleX = sUV * (sWidth * k);
            float offsetX = (float)(0.5 / dWidth - 0.5 * k);
            Vector2 scale = new Vector2(scaleX, scaleX);
            Vector2 offset = new Vector2(offsetX, offsetX);
            
            Graphics.Blit(oldHeightmap, terrainData.heightmapTexture, scale, offset);
            RenderTexture.ReleaseTemporary(oldHeightmap);

#if UNITY_2019_3_OR_NEWER
            oldHoles.filterMode = FilterMode.Point;
            Graphics.Blit(oldHoles, (RenderTexture)terrainData.holesTexture);
            RenderTexture.ReleaseTemporary(oldHoles);
#endif

            RenderTexture.active = oldRT;

            terrainData.DirtyHeightmapRegion(new RectInt(0, 0, terrainData.heightmapTexture.width, terrainData.heightmapTexture.height), TerrainHeightmapSyncControl.HeightAndLod);
#if UNITY_2019_3_OR_NEWER
            terrainData.DirtyTextureRegion(TerrainData.HolesTextureName, new RectInt(0, 0, terrainData.holesTexture.width, terrainData.holesTexture.height), false);
#endif
        }

        public const float normalizedHeightScale = 32765.0f / 65535.0f;
        /// <summary>
        /// Blit texture to Heightmap
        /// </summary>
        /// <param name="td"></param>
        /// <param name="heightmap"></param>
        public static void BlitToHeightmap(this TerrainData td, Texture2D heightmap)
        {
            var blitMat = GetBlitHeightmapMaterial();
            blitMat.SetFloat("_Height_Offset", 0 * normalizedHeightScale);
            blitMat.SetFloat("_Height_Scale", normalizedHeightScale);
            Graphics.Blit(heightmap, td.heightmapTexture, blitMat);
            td.DirtyHeightmapRegion(new RectInt(0, 0, td.heightmapResolution, td.heightmapResolution), TerrainHeightmapSyncControl.HeightAndLod);
        }

        /// <summary>
        /// return texture2d's width = heightmapResolution - 1
        /// </summary>
        /// <param name="td"></param>
        /// <returns></returns>
        public static Texture2D GetHeightmap(this TerrainData td,int resolution = -1)
        {
            var hmSize = resolution <= 0 ? td.heightmapResolution - 1 : resolution;
            var tex = new Texture2D(hmSize, hmSize, TextureFormat.R16, false, true);
            tex.BlitFrom(td.heightmapTexture);
            return tex;
        }
        /// <summary>
        /// Get holesMap use blit
        /// </summary>
        /// <param name="td"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static Texture2D GetHolesMap(this TerrainData td, int resolution = -1)
        {
            var hmSize = resolution <= 0 ? td.heightmapResolution - 1 : resolution;
            var tex = new Texture2D(hmSize, hmSize, TextureFormat.R16, false, true);
            tex.BlitFrom(td.holesTexture);
            return tex;
        }
        /// <summary>
        /// Set holesMap from texture
        /// </summary>
        /// <param name="td"></param>
        /// <param name="maskTex"></param>
        /// <param name="threshold"></param>
        /// <param name="channelId"></param>
        public static void SetHolesMap(this TerrainData td,Texture2D maskTex,float threshold = 0.5f,int channelId = 0)
        {
#if UNITY_EDITOR
            if (!maskTex.isReadable)
                maskTex = maskTex.Clone();
#endif
            var res = td.holesResolution;

            var holes = new bool[res, res];
            for (int i = 0; i < res; i++) {
                for (int j = 0; j < res; j++)
                {
                    var u = (float)i / (res - 1);
                    var v = (float)j / (res - 1);

                    var c = maskTex.GetPixelBilinear(u, v);
                    holes[j, i] = c[channelId] > threshold;
                }
            }

            td.SetHoles(0, 0, holes);
        }

        /// <summary>
        /// 
        /// terrainData's alphamapLayers must >= controlMaps.length * 4
        /// 
        /// maps : 
        /// (y)</summary>br>
        /// *
        /// *         (alphamapLayers)
        /// *       *
        /// *     *
        /// *   *
        /// * *
        /// ***************(x)
        /// </summary>
        /// <param name="td"></param>
        /// <param name="controlMaps"></param>
        public static void ApplyAlphamaps(this TerrainData td, Texture2D[] controlMaps)
        {
            if (controlMaps == null || controlMaps.Length == 0)
                return;

            // check terrain layers, one controlmap control 4 splatmaps
            var controlMapLayers = controlMaps.Length * td.alphamapLayers;
            var alphamapLayers = td.alphamapLayers;
            if (alphamapLayers < controlMapLayers)
            {
                throw new Exception(string.Format($"Warning ! terrainData's alphamapLayers < {controlMapLayers}, need add terrainLayers!"));
            }

            controlMaps = controlMaps.Where(c => c).ToArray();

            var res = td.alphamapResolution;
            float[,,] map = new float[res, res, td.alphamapLayers];

            Vector2 uv = Vector2.one;

            for (int id = 0; id < controlMaps.Length; id++)
            {
                var controlMap = controlMaps[id];
#if UNITY_EDITOR
                if (!controlMap.isReadable)
                    controlMap = controlMap.Clone();
#endif
                var colors = controlMap.GetPixels();
                var controlMapRes = controlMap.width;

                for (int y = 0; y < res; y++)
                {
                    uv.y = (float)y / res;
                    for (int x = 0; x < res; x++)
                    {
                        uv.x = (float)x / res;

                        // set alpha[x,y,z,w]
                        for (int layerId = 0; layerId < alphamapLayers; layerId++)
                        {
                            var pixelX = Mathf.FloorToInt(uv.x * controlMapRes);
                            var pixelY = Mathf.FloorToInt(uv.y * controlMapRes);
                            map[y, x, layerId] = colors[pixelX + pixelY * controlMapRes][layerId];
                        }
                    }
                }
            }
            td.SetAlphamaps(0, 0, map);
        }
        /// <summary>
        /// Set alphamap from texture
        /// </summary>
        /// <param name="td"></param>
        /// <param name="layerId"></param>
        /// <param name="tex"></param>
        /// <exception cref="Exception"></exception>
        public static void ApplyAlphamap(this TerrainData td, int layerId, Texture2D tex)
        {
#if UNITY_EDITOR
            if (!tex.isReadable)
                tex = tex.Clone();
#endif

            // check terrain layers, one controlmap control 4 splatmaps
            var controlMapLayers = td.alphamapLayers;
            var alphamapLayers = td.alphamapLayers;
            if (layerId >= td.alphamapLayers)
            {
                throw new Exception(string.Format($"layerId out of range ! terrainData's alphamapLayers: {alphamapLayers}"));
            }

            var res = td.alphamapResolution;
            float[,,] map = new float[res, res, td.alphamapLayers];

            Vector2 uv = Vector2.one;

            var colors = tex.GetPixels();
            var controlMapRes = tex.width;

            for (int y = 0; y < res; y++)
            {
                uv.y = (float)y / res;
                for (int x = 0; x < res; x++)
                {
                    uv.x = (float)x / res;

                    // set alpha[x,y,z,w]
                    {
                        var pixelX = Mathf.FloorToInt(uv.x * controlMapRes);
                        var pixelY = Mathf.FloorToInt(uv.y * controlMapRes);
                        map[y, x, layerId] = colors[pixelX + pixelY * controlMapRes][layerId];
                    }
                }
            }

            td.SetAlphamaps(0, 0, map);
        }
        /// <summary>
        /// Copy alphamaps from TerrainData
        /// </summary>
        /// <param name="td"></param>
        /// <param name="from"></param>
        public static void CopyAlphamapsFrom(this TerrainData td, TerrainData from)
        {
            if (!from)
                return;

            var maps = from.GetAlphamaps(0, 0, from.alphamapResolution, from.alphamapResolution);
            td.SetAlphamaps(0, 0, maps);
        }
        /// <summary>
        /// Noise alphamap
        /// </summary>
        /// <param name="td"></param>
        /// <param name="noiseScale"></param>
        public static void AddAlphaNoise(this TerrainData td, float noiseScale)
        {
            float[,,] maps = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);

            for (int y = 0; y < td.alphamapHeight; y++)
            {
                for (int x = 0; x < td.alphamapWidth; x++)
                {
                    float a0 = maps[x, y, 0];
                    float a1 = maps[x, y, 1];

                    a0 += Random.value * noiseScale;
                    a1 += Random.value * noiseScale;

                    float total = a0 + a1;

                    maps[x, y, 0] = a0 / total;
                    maps[x, y, 1] = a1 / total;
                }
            }

            td.SetAlphamaps(0, 0, maps);
        }

        /// <summary>
        /// Get DetailLayer to Texture
        /// </summary>
        /// <param name="td"></param>
        /// <param name="layerId"></param>
        /// <returns></returns>
        public static Texture2D GetDetailLayerTexture(this TerrainData td, int layerId)
        {
            var map = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, layerId);
            var splatCount = td.detailPrototypes.Length;
            var tex = new Texture2D(td.detailWidth, td.detailHeight) { name = $"detail {layerId}" };
            for (int y = 0; y < td.detailHeight; y++)
            {
                for (int x = 0; x < td.detailWidth; x++)
                {
                    var c = map[y, x] / (float)splatCount;
                    tex.SetPixel(x, y, new Color(c, c, c));
                }
            }
            tex.Apply();
            return tex;
        }
        /// <summary>
        /// Set DetailLayer from texture
        /// </summary>
        /// <param name="td"></param>
        /// <param name="layerId"></param>
        /// <param name="tex"></param>
        public static void SetDetailLayerTexture(this TerrainData td, int layerId, Texture2D tex,float threshold = 0.5f, float maxDensity=100)
        {
#if UNITY_EDITOR
            if (!tex.isReadable)
                tex = tex.Clone();
#endif
            int[,] map = new int[td.detailHeight, td.detailWidth];
            for (int y = 0; y < td.detailHeight; y++)
            {
                for (int x = 0; x < td.detailWidth; x++)
                {
                    var col = tex.Sample(x, y, td.detailWidth, td.detailHeight);
                    map[y, x] = Mathf.FloorToInt(Mathf.Clamp01(col.r - threshold) * maxDensity);
                }
            }
            td.SetDetailLayer(0, 0, layerId, map);
        }
    }
}
