namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using PowerUtilities;
    using System;

    public class MeshCombineContextMenu : MonoBehaviour
    {
        public const string ROOT_PATH = "PowerUtilities/网格工具";

        [MenuItem(ROOT_PATH + "/合并选择的网格")]
        static void CombineSelected()
        {
            var gos = Selection.gameObjects;
            gos.Select(go => go.GetComponentsInChildren<MeshRenderer>())
                .SelectMany(rs => rs)
                .Where(r => r.GetComponent<MeshFilter>())
                .GroupBy(r => r.sharedMaterial)
                .ForEach(group =>
                {
                //按顶点数量来分段.
                var meshFilters = group.Select(mr => mr.GetComponent<MeshFilter>())
                    .OrderByDescending(mf => mf.sharedMesh.vertexCount).ToList();

                    var combinedGoList = MeshTools.CombineGroupMeshes(meshFilters, group.Key);
                    var vertexCount = combinedGoList.Aggregate(0, (c, go) => c += go.GetComponent<MeshFilter>().sharedMesh.vertexCount);

                    var rootGo = new GameObject(string.Format("{0},vertex_{1}", group.Key.name, vertexCount));
                    combinedGoList.ForEach(cgo =>
                    {
                        cgo.transform.SetParent(rootGo.transform);
                    });

                });
        }

        [MenuItem(ROOT_PATH + "/分离多材质网格")]
        static void SplitSelected()
        {
            var gos = Selection.gameObjects;
            gos.Select(go => go.GetComponentsInChildren<MeshFilter>())
                .SelectMany(rs => rs)
                .Where(mf => mf.sharedMesh && mf.sharedMesh.subMeshCount > 1)
                .ForEach(mf => MeshTools.SplitMesh(mf.gameObject));
        }

        [MenuItem(ROOT_PATH + "/统计顶点数")]
        static void StatisticsVertexCount()
        {
            var gos = Selection.gameObjects;
            var count = gos.Select(go => go.GetComponentsInChildren<MeshFilter>())
            .SelectMany(rs => rs)
            .Where(mf => mf.sharedMesh)
            .Aggregate(0, (c, mf) => c += mf.sharedMesh.vertexCount);

            EditorUtility.DisplayDialog("统计", "顶点数 : " + count, "ok");
        }
    }
#endif
}