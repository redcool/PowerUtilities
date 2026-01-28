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
    using System.IO;

    public class MeshCombineContextMenu : MonoBehaviour
    {
        public const string ROOT_PATH = "PowerUtilities/网格工具";

        [MenuItem(ROOT_PATH + "/合并选择的网格")]
        static void CombineSelected()
        {
            var gos = Selection.gameObjects;
            var renderers = gos.SelectMany(go => go.GetComponentsInChildren<MeshRenderer>()).ToArray();

            if (renderers.Length == 0)
            {
                EditorTools.DisplayDialog_Ok_Cancel("MeshRenderer not found");
                return;
            }
            var goName = $"MeshGroup {renderers.Length}";
            var parentGo = GameObject.Find(goName) ?? new GameObject(goName);

            var meshFilterList = MeshTools.CombineMeshesGroupByMaterial(renderers, parentGo.transform, false, false, false, "EditorOnly");

            // save meshes
            CombineChildrenMeshGroupByMaterial.SaveMeshFilters(meshFilterList);
        }

        [MenuItem(ROOT_PATH + "/分离多材质网格")]
        static void SplitSelected()
        {
            var gos = Selection.gameObjects;
            var mfs = gos.SelectMany(go => go.GetComponentsInChildren<MeshFilter>())
                .Where(mf => mf.sharedMesh && mf.sharedMesh.subMeshCount > 1)
                .ToArray();

            if (mfs.Length == 0)
            {
                EditorTools.DisplayDialog_Ok_Cancel("Multi material mesh not found");
                return;
            }

            foreach (var mf in mfs)
            {
                var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(mf.sharedMesh));
                MeshTools.SplitMesh(mf.gameObject, (list =>
                    {
                        if (string.IsNullOrEmpty(folder))
                            return;

                        foreach (var childGo in list)
                        {
                            var childMesh = childGo.GetComponent<MeshFilter>().sharedMesh;
                            AssetDatabase.CreateAsset(childMesh, $"{folder}/{childGo.name}.asset");

                            //var prefabPath = $"{folder}/{childGo.name}.prefab";
                            //PrefabTools.CreatePrefab(childGo, prefabPath);
                        }
                    }));
            }
            AssetDatabaseTools.SaveRefresh();
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