namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    public static class MeshTools
    {

        static List<List<MeshFilter>> GroupByVertexCount(List<MeshFilter> meshList,int maxCount=65000)
        {
            var segments = new List<List<MeshFilter>>();

            var total = 0;
            // insert first
            var segment = new List<MeshFilter>();
            segments.Add(segment);

            for (int i = 0; i < meshList.Count; i++)
            {
                var mf = meshList[i];
                total += mf.sharedMesh.vertexCount;
                if (total > maxCount)
                {
                    segment = new List<MeshFilter>();
                    segments.Add(segment);
                    total = 0;
                    i--;
                }else
                    segment.Add(mf);
            }

            return segments;
        }

        public static List<GameObject> CombineGroupMeshes(List<MeshFilter> groupMeshList, Material groupMat)
        {
            var segments = GroupByVertexCount(groupMeshList);
            
            return segments.Select( (segment,id)=> {
                var mesh = CombineMeshInstance(segment);
                var go = new GameObject(string.Format("{0} - {1}",groupMat.name,id));
                go.AddComponent<MeshRenderer>().sharedMaterial = groupMat;
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                return go;
            }).ToList();
        }

        public static Mesh CombineMeshInstance(List<MeshFilter> mfs)
        {
            var q = mfs.Select(mf =>
            {
                var mr = mf.GetComponent<MeshRenderer>();
                var ci = new CombineInstance
                {
                    lightmapScaleOffset = mr.lightmapScaleOffset,
                    mesh = mf.sharedMesh,
                    realtimeLightmapScaleOffset = mr.realtimeLightmapScaleOffset,
                    transform = mr.transform.localToWorldMatrix
                };
                return ci;
            });
            var m = new Mesh();
            m.CombineMeshes(q.ToArray(), true, true, true);
            return m;
        }

        public static Mesh CombineMesh(List<MeshFilter> mfs)
        {
            var verts = new List<Vector3>();
            var uv = new List<Vector2>();
            var uv2 = new List<Vector2>();
            var colors = new List<Color>();
            var triangles = new List<int>();

            var vertexCount = 0;
            foreach (var mf in mfs)
            {
                var m = mf.sharedMesh;
                verts.AddRange(m.vertices.Select(v => mf.transform.localToWorldMatrix.MultiplyPoint(v)).ToArray());
                uv.AddRange(m.uv);
                uv2.AddRange(m.uv2.Length > 0 ? m.uv2 : m.vertices.Select(v => Vector2.zero));
                colors.AddRange(m.colors.Length > 0 ? m.colors : m.vertices.Select(v => Color.white).ToArray());
                triangles.AddRange(m.triangles.Select(vertexId => vertexId + vertexCount).ToArray());

                vertexCount += m.vertexCount;
                if(colors.Count != 0 && colors.Count != verts.Count)
                {
                    Debug.Log("oops");
                }
            }

            var mesh = new Mesh();
            mesh.vertices = verts.ToArray();
            mesh.uv = uv.ToArray();
            mesh.uv2 = uv2.ToArray();
            mesh.colors = colors.ToArray();
            mesh.triangles = triangles.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            return mesh;
        }

        /// <summary>
        //  多材质的mesh分离为多个mesh
        /// </summary>
        /// <param name="go"></param>
        /// <param name="onSplited"></param>
        public static void SplitMesh(GameObject go, Action<List<GameObject>> onSplited = null)
        {
            var mr = go.GetComponent<MeshRenderer>();
            var mf = go.GetComponent<MeshFilter>();
            var mats = mr.sharedMaterials;

            var list = new List<GameObject>();

            var mesh = mf.sharedMesh;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                var newMesh = SplitMesh(mesh, i);

                var newGo = new GameObject(string.Format("subMesh {0}",i));
                list.Add(newGo);
                newGo.AddComponent<MeshFilter>().sharedMesh = newMesh;
                newGo.AddComponent<MeshRenderer>().sharedMaterial = mats[i];

                newGo.transform.SetParent(go.transform,false);
            }

            if (onSplited != null)
                onSplited(list);
        }


        public static Mesh SplitMesh(Mesh originalMesh, int subMeshId)
        {
            Vector3[] verts;
            Vector2[] uv, uv2;
            Color[] colors;
            int[] triangles;
            SplitMesh(originalMesh, subMeshId, out verts, out triangles, out uv, out uv2, out colors);

            var newMesh = new Mesh();
            newMesh.vertices = verts;
            newMesh.triangles = triangles;
            newMesh.colors = colors;
            newMesh.uv = uv;
            newMesh.uv2 = uv2;

            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            return newMesh;
        }

        /// <summary>
        /// 独立出originalMesh的submesh
        /// </summary>
        /// <param name="originalMesh"></param>
        /// <param name="subMeshId"></param>
        /// <param name="verts"></param>
        /// <param name="triangles"></param>
        /// <param name="uv"></param>
        /// <param name="uv2"></param>
        /// <param name="colors"></param>
        public static void SplitMesh(Mesh originalMesh, int subMeshId,
            out Vector3[] verts, out int[] triangles, out Vector2[] uv, out Vector2[] uv2, out Color[] colors)
        {
            var refTriangles = originalMesh.GetTriangles(subMeshId);
            verts = new Vector3[refTriangles.Length];
            uv = new Vector2[refTriangles.Length];
            uv2 = new Vector2[refTriangles.Length];
            colors = new Color[refTriangles.Length];
            triangles = new int[refTriangles.Length];

            for (int i = 0; i < refTriangles.Length; i++)
            {
                var refTriangle = refTriangles[i];

                triangles[i] = i;
                verts[i] = originalMesh.vertices[refTriangle];
                uv[i] = originalMesh.uv[refTriangle];
                uv2[i] = originalMesh.uv2[refTriangle];
                colors[i] = originalMesh.colors[refTriangle];
            }
        }

        public static void CopyFrom(this Mesh a,Mesh b)
        {
            if (!a || !b)
                return;
            
            a.Clear();
            a.vertices = b.vertices;
            a.uv = b.uv;
            a.uv2 = b.uv2;
            a.uv3 = b.uv3;
            a.uv4 = b.uv4;
            a.uv5 = b.uv5;
            a.uv6 = b.uv6;
            a.uv7 = b.uv7;
            a.uv8 = b.uv8;
            a.colors = b.colors;
            a.triangles = b.triangles;
            a.normals = b.normals;
            a.tangents = b.tangents;
            a.bindposes = b.bindposes;
            a.boneWeights = b.boneWeights;
            a.bounds = b.bounds;
            a.subMeshCount = b.subMeshCount;
            a.indexFormat = b.indexFormat;
#if UNITY_2021_1_OR_NEWER
            a.indexBufferTarget = b.indexBufferTarget;
#endif
        }
        /// <summary>
        /// Get bone start index for per vertex,index is mesh.GetAllBoneWeights
        /// </summary>
        /// <param name="bonesPerVertex"></param>
        /// <returns></returns>
        public static int[] GetBoneStartPerVertex(this Mesh mesh)
        {
            var bonesPerVertex = mesh.GetBonesPerVertex();
            var bonesStartPerVertex = new int[bonesPerVertex.Length];
            
            var startIndex = 0;

            for (int i = 0; i < bonesPerVertex.Length; i++)
            {
                bonesStartPerVertex[i] = startIndex;
                var count = bonesPerVertex[i];

                startIndex += count;
            }
            return bonesStartPerVertex;
        }
        /// <summary>
        /// Get PerVertex's BoneWeight1 array
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static (byte count, int start)[] GetBoneWeight1_InfoPerVertex(this Mesh mesh)
        {
            var weights = mesh.GetAllBoneWeights();
            var bonesStartIndexPerVertex = mesh.GetBoneStartPerVertex();
            var bonesPerVertex = mesh.GetBonesPerVertex();

            var boneInfoPerVertices = bonesPerVertex
                    .Zip(bonesStartIndexPerVertex, (count, start) => (count, start))
                    .ToArray();
            return boneInfoPerVertices;
        }
    }
}