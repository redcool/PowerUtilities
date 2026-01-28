#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// MeshFilter no MeshFilterEditor
    /// </summary>
    [CustomEditor(typeof(MeshFilter))]
    public class MeshFilterEditorEx : Editor
    {
        private bool isFold;
        readonly GUIContent meshToolsGUI = new GUIContent("Mesh Tools","Operate mesh");
        readonly GUIContent splitSubMeshesGUI = new GUIContent("Split SubMeshes","Split submeshes into multiple meshes");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUITools.DrawColorLine(1);

            var meshFilter = target as MeshFilter;
            using (new EditorGUI.DisabledGroupScope(meshFilter.sharedMesh == null))
            {
                EditorGUITools.BeginFoldoutHeaderGroupBox(ref isFold, meshToolsGUI, () =>
                {
                    if (GUILayout.Button(splitSubMeshesGUI))
                    {
                        var dirPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(meshFilter.sharedMesh));
                        
                        MeshTools.SplitMesh(meshFilter.gameObject, list =>
                        {
                            if (string.IsNullOrEmpty(dirPath))
                                return;

                            foreach (var childGo in list)
                            {
                                var childMesh = childGo.GetComponent<MeshFilter>().sharedMesh;
                                AssetDatabase.CreateAsset(childMesh, $"{dirPath}/{childGo.name}.asset");

                                //var prefabPath = $"{dirPath}/{childGo.name}.prefab";
                                //PrefabTools.CreatePrefab(childGo, prefabPath);
                            }
                        });
                    }
                });
            }
        }
    }
}
#endif