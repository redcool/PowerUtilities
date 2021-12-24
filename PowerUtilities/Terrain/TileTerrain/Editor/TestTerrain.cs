namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;
    using PowerUtilities;
    using System.Runtime.InteropServices;

    [CustomEditor(typeof(TestTerrain))]
    public class TestTerrainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("mat"));

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Show"))
            {
                var inst = target as TestTerrain;

                GenTerrain(Terrain.activeTerrain, inst.mat, 2, 2, 2);
            }
        }

        private void GenTerrain(Terrain terrain, Material mat, int resScale, int xCount, int zCount)
        {
            var td = terrain.terrainData;
            var resolution = td.heightmapResolution - 1;
            var tileWidth = resolution / xCount;
            var tileHeight = resolution / zCount;

            for (int z = 0; z < xCount; z++)
            {
                for (int x = 0; x < zCount; x++)
                {
                    var heightmapRect = new RectInt(x * tileWidth, z * tileWidth, tileWidth + 1, tileHeight + 1);
                    //Debug.Log(heightmapRect);
                    GenTerrain(Terrain.activeTerrain, mat, heightmapRect, resScale);
                    //return;
                }
            }
        }

        private void GenTerrain(Terrain terrain, Material mat, RectInt heightmapRect, int resScale)
        {
            //heightmapRect = new RectInt(256, 0, 257, 257);
            // mesh scale
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

            var go = new GameObject("TerrainMesh");
            go.AddComponent<MeshFilter>().mesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }
    }
    public class TestTerrain : MonoBehaviour
    {
        public Material mat;
    }

#endif
}