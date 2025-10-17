using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PowerUtilities
{

    public class CombineChildrenMeshGroupByMaterial : MonoBehaviour
    {
        [Tooltip("Include inactive children mesh renderer ")]
        public bool isIncludeInactive;

        [Tooltip("combine meshes when start")]
        public bool isCombineOnStart = true;

        [Tooltip("disable orignal renderers when combined")]
        public bool isDisableOriginalRenderers = true;
        // Start is called before the first frame update
        void Start()
        {
            if (isCombineOnStart)
                CombineMeshesGroupByMaterial(gameObject, isDisableOriginalRenderers, isIncludeInactive);
        }

        /// <summary>
        /// Combine meshes for same material 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="isIncludeInactive"></param>
        public static void CombineMeshesGroupByMaterial(GameObject root, bool isDisableRenderer, bool isIncludeInactive = false)
        {
            var mrs = root.GetComponentsInChildren<MeshRenderer>(isIncludeInactive);
            if (mrs.Length == 0)
                return;

            List<Vector3> vertices = new();
            List<int> triangles = new();
            List<Vector3> normals = new();
            List<Vector2> uvs0 = new();
            List<Color> colors = new();

            var groupDict = mrs
                .Where(mr =>
                {
                    var mf = mr.GetComponent<MeshFilter>();
                    return mf && mf.sharedMesh;
                })
                .GroupBy(mr => mr.sharedMaterial)
                .ToDictionary(g => g.Key, g => g.ToList())
                ;
            var vertexOffset = 0;
            var groupId = 0;
            foreach (var group in groupDict)
            {
                var mat = group.Key;
                var renderers = group.Value;
                var meshId = 0;

                foreach (var renderer in renderers)
                {
                    var mf = renderer.GetComponent<MeshFilter>();
                    var childMesh = mf.sharedMesh;

                    vertices.AddRange(childMesh.vertices.Select(v => mf.transform.TransformPoint(v)));

                    normals.AddRange(childMesh.normals.Select(n => mf.transform.TransformDirection(n)));
                    uvs0.AddRange(childMesh.uv);
                    colors.AddRange(Enumerable.Repeat(Random.ColorHSV(), childMesh.vertexCount));
                    triangles.AddRange(childMesh.triangles.Select(id => id + vertexOffset));
                    vertexOffset += childMesh.vertexCount;

                    meshId++;

                    renderer.enabled = !isDisableRenderer;
                }

                var bigMesh = new Mesh();
                bigMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                bigMesh.SetVertices(vertices);
                bigMesh.SetNormals(normals);
                bigMesh.SetColors(colors);
                bigMesh.SetTriangles(triangles, 0);
                bigMesh.SetUVs(0, uvs0);

                bigMesh.RecalculateBounds();
                bigMesh.RecalculateNormals();
                bigMesh.Optimize();

                var go = new GameObject("group"+ groupId);
                go.AddComponent<MeshFilter>().sharedMesh = bigMesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = mat;
                go.transform.parent = root.transform;

                groupId++;
            }
        }
    }
}
