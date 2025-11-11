namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using UnityEditor.EditorTools;
    using UnityEngine;
    using UnityEngine.TerrainTools;
    using Object = UnityEngine.Object;
    using Random = UnityEngine.Random;

    public static class TerrainTools
    {
        private const string SET_EXACT_HEIGHT_SHADER = "Hidden/TerrainTools/SetExactHeight";
        private const string PAINT_HEIGHT_SHADER = "Hidden/TerrainEngine/PaintHeight";
        private const string BRUSH_PREVIEW_SHADER = "Hidden/TerrainEngine/BrushPreview";
        private const string BRUSH_PREVIEW_EX_SHADER = "Hidden/TerrainEngine/BrushPreviewEX";

        static Dictionary<string, Material> terrainMatDict = new Dictionary<string, Material>();


        public static void CopyFromTerrain(MeshRenderer mr, Terrain terrain)
        {
            mr.sharedMaterial = terrain.materialTemplate;
            mr.lightmapIndex = terrain.lightmapIndex;
            mr.lightmapScaleOffset = terrain.lightmapScaleOffset;
            mr.realtimeLightmapIndex = terrain.realtimeLightmapIndex;
            mr.realtimeLightmapScaleOffset = mr.realtimeLightmapScaleOffset;
        }
#if UNITY_2018_3_OR_NEWER
        public static void ExtractAlphaMapToPNG(Terrain terrain, string path)
        {
            var tex = GetBlendSplatMap(terrain);
            File.WriteAllBytes(path, tex.EncodeToPNG());
        }

        public static Texture2D GetBlendSplatMap(Terrain terrain, int baseMapSize = 1024)
        {
            var td = terrain.terrainData;
            var maps = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);

            var nx = 1f / baseMapSize;
            var ny = 1f / baseMapSize;

            var tex = new Texture2D(baseMapSize, baseMapSize, TextureFormat.RGBA32, true);
            for (int x = 0; x < tex.width; x++)
            {
                for (int y = 0; y < tex.height; y++)
                {
                    int alphaMapCoordX = (int)(((float)x / tex.width) * td.alphamapWidth);
                    int alphaMapCoordY = (int)(((float)y / tex.height) * td.alphamapHeight);

                    var finalColor = new Color();

                    for (int z = 0; z < td.alphamapLayers; z++)
                    {
                        var alpha = maps[alphaMapCoordY, alphaMapCoordX, z];
                        var layer = td.terrainLayers[z];
                        var tile = new Vector2(td.size.x, td.size.z) / layer.tileSize;

                        var diff = layer.diffuseTexture;

                        var u = x * nx * tile.x;
                        var v = y * ny * tile.y;

                        //var px = Mathf.FloorToInt(u * diff.width);
                        //var py = Mathf.FloorToInt(v * diff.height);
                        //var c = diff.GetPixel(px, py);

                        var c = diff.GetPixelBilinear(u, v);
                        finalColor += c * alpha;
                    }
                    tex.SetPixel(x, y, finalColor);
                }
            }

            return tex;
        }
        /// <summary>
        /// Generates a collection of tile meshes from the specified terrain, dividing it into a grid of tiles.
        /// </summary>
        /// <remarks>The terrain is divided into a grid of tiles, with each tile represented by a mesh.
        /// The number of tiles is determined by the <paramref name="tileRowCount"/> parameter, which specifies the
        /// number of tiles along one side of the grid. The total number of tiles will be <paramref
        /// name="tileRowCount"/> squared.  The <paramref name="saveResolution"/> parameter controls the resolution of
        /// the generated meshes. A value of 1 retains the full resolution, while higher values reduce the resolution
        /// proportionally.  Each generated mesh is named using the format "Tile-X_Y", where X and Y represent the
        /// tile's position in the grid.</remarks>
        /// <param name="terrain">The terrain from which the tile meshes will be generated. Cannot be <see langword="null"/>.</param>
        /// <param name="tileRowCount">The number of tiles along one side of the grid. Must be a power of two. If not, the closest power of two
        /// greater than or equal to the value will be used.</param>
        /// <param name="saveResolution">The resolution scale factor for the generated meshes. A higher value reduces the resolution of the meshes,
        /// improving performance at the cost of detail. Defaults to 1.</param>
        /// <returns>A list of <see cref="Mesh"/> objects representing the generated tile meshes. Each mesh corresponds to a tile
        /// in the grid.</returns>
        public static List<Mesh> GenerateTileMeshes(Terrain terrain, int tileRowCount, int saveResolution=1)
        {
            tileRowCount = Mathf.Max(1, Mathf.ClosestPowerOfTwo(tileRowCount)); // need pow of 2
            var resScale = (int)Mathf.Pow(2, Mathf.Max(0, saveResolution));

            var list = new List<Mesh>();

            var td = terrain.terrainData;
            var heightmapSize = (td.heightmapResolution - 1) / tileRowCount;

            var count = tileRowCount * tileRowCount;

            for (int x = 0; x < tileRowCount; x++)
            {
                for (int z = 0; z < tileRowCount; z++)
                {
                    var heightmapRect = new RectInt(x * heightmapSize, z * heightmapSize, heightmapSize + 1, heightmapSize + 1);
                    var tileMesh = GenerateTileMesh(terrain, heightmapRect, resScale);
                    tileMesh.name = $"Tile-{x}_{z}";

                    list.Add(tileMesh);
                }
            }
            return list;
        }

        /// <summary>
        /// Generates a 3D mesh representing a tile of terrain based on the specified heightmap region and resolution
        /// scale.
        /// </summary>
        /// <remarks>The generated mesh is scaled according to the terrain's heightmap scale, and the UV
        /// coordinates are adjusted to fit within the appropriate tile region. This method is useful for creating tiled
        /// terrain meshes for rendering or physics simulations.</remarks>
        /// <param name="terrain">The <see cref="Terrain"/> object from which the heightmap data is retrieved.</param>
        /// <param name="heightmapRect">A <see cref="RectInt"/> defining the region of the heightmap to use for generating the mesh. The width and
        /// height of the rectangle determine the resolution of the mesh.</param>
        /// <param name="resScale">The resolution scale factor. A higher value reduces the number of vertices in the mesh, resulting in lower
        /// detail. Must be a positive integer.</param>
        /// <returns>A <see cref="Mesh"/> object representing the generated terrain tile. The mesh includes vertices, UVs, and
        /// triangles based on the specified heightmap region and resolution scale.</returns>
        public static Mesh GenerateTileMesh(Terrain terrain, RectInt heightmapRect, int resScale)
        {
            var td = terrain.terrainData;
            var meshScale = td.heightmapScale;
            meshScale.y = 1;

            //uv scale
            var hw = heightmapRect.width;
            var hh = heightmapRect.height;
            var resolution = heightmapRect.width - 1;

            var tileId = new Vector2(heightmapRect.x / (heightmapRect.width - 1), heightmapRect.y / (heightmapRect.height - 1));

            var uvTileCount = td.heightmapResolution / resolution;
            var uvTileRate = 1f / uvTileCount;
            var uvScale = new Vector2(uvTileRate / resolution * resScale, uvTileRate / resolution * resScale);

            // triangles 
            hh = (hh - 1) / resScale + 1;
            hw = (hw - 1) / resScale + 1;
            resolution /= resScale;

            //for (int z = 0; z < hh; z++)
            //{
            //    for (int x = 0; x < hw; x++)
            //    {
            //        var offsetX = (x * resScale + heightmapRect.x);
            //        var offsetZ = (z * resScale + heightmapRect.y);
            //        var y = td.GetHeight(offsetX * 1, offsetZ * 1);
            //        var pos = Vector3.Scale(new Vector3(offsetX, y, offsetZ), meshScale);
            //        Debug.DrawRay(pos, Vector3.up, Color.green, 1);
            //    }
            //}
            //return;

            //heightmapRect = new RectInt(0, 0, td.heightmapResolution, td.heightmapResolution);
            //resScale = 4;

            var verts = new Vector3[hw * hh];
            var uvs = new Vector2[verts.Length];
            var triangles = new int[resolution * resolution * 6];
            var vertexIndex = 0;
            var triangleIndex = 0;


            for (int z = 0; z < hh; z++)
            {
                for (int x = 0; x < hw; x++)
                {
                    var offsetX = x * resScale + heightmapRect.x;
                    var offsetZ = z * resScale + heightmapRect.y;

                    var y = td.GetHeight(offsetX, offsetZ);
                    var pos = Vector3.Scale(new Vector3(offsetX, y, offsetZ), meshScale);

                    verts[vertexIndex] = pos;
                    uvs[vertexIndex] = Vector2.Scale(new Vector2(x, z), uvScale) + tileId * uvTileRate;
                    vertexIndex++;

                    //Debug.DrawRay(pos, Vector3.up, Color.green,1);

                    if (x < resolution && z < resolution)
                    {
                        /**
                         c d
                         a b
                         */
                        var a = z * hw + x;
                        var b = a + 1;
                        var c = (z + 1) * hw + x;
                        var d = c + 1;

                        triangles[triangleIndex++] = a;
                        triangles[triangleIndex++] = c;
                        triangles[triangleIndex++] = d;

                        triangles[triangleIndex++] = a;
                        triangles[triangleIndex++] = d;
                        triangles[triangleIndex++] = b;
                    }
                }
            }

            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        public static void GenerateWhole(Terrain terrain, Transform paretTr, int resScale = 1)
        {
            var td = terrain.terrainData;
            var hw = td.heightmapResolution;
            var hh = td.heightmapResolution;

            var resolution = td.heightmapResolution - 1;

            var meshScale = td.heightmapScale * resScale;
            meshScale.y = 1;

            var uvScale = new Vector2(1f / resolution * resScale, 1f / resolution * resScale);
            hh = (hh - 1) / resScale + 1;
            hw = (hw - 1) / resScale + 1;
            resolution /= resScale;

            var verts = new Vector3[hw * hh];
            var uvs = new Vector2[verts.Length];
            var triangles = new int[resolution * resolution * 6];
            var vertexIndex = 0;
            var triangleIndex = 0;


            for (int z = 0; z < hh; z++)
            {
                for (int x = 0; x < hw; x++)
                {
                    var y = td.GetHeight(x * resScale, z * resScale);

                    verts[vertexIndex] = Vector3.Scale(new Vector3(x, y, z), meshScale);
                    uvs[vertexIndex] = Vector2.Scale(new Vector2(x, z), uvScale);
                    vertexIndex++;

                    if (x < resolution && z < resolution)
                    {
                        /**
                         c d
                         a b
                         */
                        var a = z * hw + x;
                        var b = a + 1;
                        var c = (z + 1) * hw + x;
                        var d = c + 1;

                        triangles[triangleIndex++] = a;
                        triangles[triangleIndex++] = c;
                        triangles[triangleIndex++] = d;

                        triangles[triangleIndex++] = a;
                        triangles[triangleIndex++] = d;
                        triangles[triangleIndex++] = b;
                    }
                }
            }


            var mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = verts;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            var go = new GameObject("Terrain Mesh");
            go.transform.SetParent(paretTr, false);
            go.AddComponent<MeshFilter>().mesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            CopyFromTerrain(mr, terrain);
        }


#endif

        public static List<Terrain> GenerateTerrainsByHeightmaps(Transform rootTr, List<Texture2D> heightmaps, int countInRow, Vector3 terrainSize, Material materialTemplate)
        {
            if (heightmaps == null)
                return null;

            // cleanup
            foreach (Transform item in rootTr)
            {
                Object.DestroyImmediate(item.gameObject);
            }

            // calc rows
            var count = heightmaps.Count;
            var rows = count / countInRow;
            if (count % countInRow > 0)
                rows++;

            // get root go
            var terrainRootGo = new GameObject("Terrains");
            terrainRootGo.transform.SetParent(rootTr, false);

            // generate terrain go
            var heightMapId = 0;
            var terrainList = new List<Terrain>();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < countInRow; x++)
                {
                    var go = new GameObject(string.Format("Terrain Tile [{0},{1}]", x, y));
                    var t = go.AddComponent<Terrain>();
                    terrainList.Add(t);

                    t.terrainData = new TerrainData();
                    t.terrainData.ApplyHeightmap(heightmaps[heightMapId++]);
                    t.terrainData.size = terrainSize;

                    t.transform.SetParent(terrainRootGo.transform, false);
                    t.transform.position = Vector3.Scale(terrainSize, new Vector3(x, 0, y));

                    var c = go.AddComponent<TerrainCollider>();
                    c.terrainData = t.terrainData;

                    t.materialTemplate = materialTemplate;
                }
                //break;
            }
            return terrainList;
        }



        public static void AutoSetNeighbours(Terrain[] terrains, int tilesX, int tilesZ)
        {
            if (terrains == null || terrains.Length == 0)
                return;
            // set neighbor terrains to update normal maps
            for (int y = 0; y < tilesZ; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    int index = (y * tilesX) + x;
                    Terrain terrain = terrains[index];
                    Terrain leftTerrain = (x > 0) ? terrains[index - 1] : null;
                    Terrain rightTerrain = (x < tilesX - 1) ? terrains[index + 1] : null;
                    Terrain topTerrain = (y > 0) ? terrains[index - tilesX] : null;
                    Terrain bottomTerrain = (y < tilesZ - 1) ? terrains[index + tilesX] : null;

                    // NOTE: "top" and "bottom" are reversed because of the way the terrain is handled...
                    terrain.SetNeighbors(leftTerrain, bottomTerrain, rightTerrain, topTerrain);
                }
            }
        }

        public static void AutoSetNeighbours(Terrain[] terrains, int countInRow)
        {
            if (terrains == null)
                return;

            var tileY = terrains.Length / countInRow;
            for (int y = 0; y < tileY; y++)
            {
                for (int x = 0; x < countInRow; x++)
                {
                    var id = x + y * countInRow;
                    var t = terrains[id];
                    var leftId = (x - 1) + y * countInRow;
                    var rightId = (x + 1) + y * countInRow;
                    var topId = x + (y + 1) * countInRow;
                    var bottomId = x + (y - 1) * countInRow;

                    Debug.Log($"id:{id},r:{rightId},b:{bottomId},l:{leftId},t:{topId}");
                }
            }
        }

        /// <summary>
        /// terrainUV : uv[0,1] on terrain
        /// </summary>
        /// <param name="t"></param>
        /// <param name="terrainUV"></param>
        /// <returns></returns>
        public static Vector3 TerrainUVToWorldPos(this Terrain t, Vector3 terrainUV)
        {
            var v = t.GetPosition();
            v += Vector3.Scale(terrainUV, t.terrainData.size);

            return v;
        }
        public static Vector2 WorldPosToTerrainUV(this Terrain t, Vector3 worldPos)
        {
            var localPos = t.transform.InverseTransformPoint(worldPos);
            var td = t.terrainData;
            var uv = new Vector2(localPos.x / td.size.x, localPos.z / td.size.z);
            return uv;
        }

        public static Material GetTerrainMat(string shaderName, string defaultShaderWhenNotFound = default)
        {
            if (!terrainMatDict.TryGetValue(shaderName, out var mat))
            {
                var shader = Shader.Find(shaderName) ?? Shader.Find(defaultShaderWhenNotFound);

                mat = new Material(shader);
                terrainMatDict.Add(shaderName, mat);
            }
            return mat;
        }

        /// <summary>
        /// need package : com.unity.terrain-tools
        /// </summary>
        /// <returns></returns>
        public static Material Get_SetExactHeightMat() => GetTerrainMat(SET_EXACT_HEIGHT_SHADER);
        public static Material GetBuiltinPaintMaterial() => GetTerrainMat(PAINT_HEIGHT_SHADER);
        public static Material GetDefaultBrushPreviewMaterial() => GetTerrainMat(BRUSH_PREVIEW_SHADER);
        public static Material GetDefaultBrushPreviewExMaterial() => GetTerrainMat(BRUSH_PREVIEW_EX_SHADER, BRUSH_PREVIEW_SHADER);


        public static bool GetHitInfo(Vector3 pos, out RaycastHit hitInfo)
        {
            var ray = new Ray(pos, Vector3.down);
            return Physics.Raycast(ray, out hitInfo, float.MaxValue);
        }

        // Ease of use function for rendering modified Terrain Texture data into a PaintContext. This is used in both OnRenderBrushPreview and OnPaint.
        public static void RenderIntoPaintContext(PaintContext paintContext, BrushTransform brushXform, Material mat)
        {
            // Setup the material for reading from/writing into the PaintContext texture data. This is a necessary step to setup the correct shader properties for appropriately transforming UVs and sampling textures within the shader
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            // Render into the PaintContext's destinationRenderTexture using the built-in painting Material - the id for the Raise/Lower pass is 0.
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        }

        /// <summary>
        /// Setup Terrain PaintMat's params, (GetBuiltinPaintMaterial() , Get_SetExactHeightMat)
        /// </summary>
        /// <param name="paintMat"></param>
        /// <param name="brushTexture"></param>
        /// <param name="brushParams"></param>
        /// <param name="filterTexture"></param>
        public static void SetupTerrainPaintMat(ref Material paintMat, Texture brushTexture, Vector4 brushParams, Texture filterTexture = null)
        {
            paintMat.SetTexture("_BrushTex", brushTexture);
            paintMat.SetVector("_BrushParams", brushParams);
            paintMat.SetTexture("_FilterTex", filterTexture ?? Texture2D.whiteTexture);
        }
    }
}