namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System;
    using Object = UnityEngine.Object;
    using System.Text;
    using System.IO;
    using System.Threading;

    /// <summary>
    /// 根据当前选择的材质,找出关联的prefabs
    /// </summary>
    public class MaterialAnalysisWindow : EditorWindow
    {
        public const string MENU_PATH = "PowerUtilities/分析工具/Material分析";
        private Vector2 scrollPosition;

        private List<GameObject> results = new List<GameObject>();

        Material targetMat;

        [MenuItem(MENU_PATH + "/查看材质引用的prefab")]
        static void Init()
        {
            var win = GetWindow<MaterialAnalysisWindow>();
            win.Show();
        }

        void OnGUI()
        {

            targetMat = (Material)EditorGUILayout.ObjectField("目标材质:", targetMat, typeof(Material), false);
            if (!targetMat)
                return;

            EditorGUILayout.BeginVertical("box");
            if (GUILayout.Button("搜索"))
            {
                results = GetItemsUseMaterial(targetMat);
            }
            EditorGUILayout.EndVertical();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, "box");
            EditorGUILayout.BeginVertical("box");
            DrawGameObjects();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        List<List<string>> SplitToBlocks(string[] guids)
        {
            var count = Mathf.CeilToInt(guids.Length / 100f);
            var blocks = new List<List<string>>();

            List<string> blockList = null;
            for (int i = 0; i < guids.Length; i++)
            {
                if (i % 100 == 0)
                {
                    blockList = new List<string>();
                    blocks.Add(blockList);
                }
                blockList.Add(guids[i]);
            }

            return blocks;
        }

        private void DrawGameObjects()
        {
            if (results.Count == 0)
            {
                EditorGUILayout.HelpBox("没有发现使用该材质的prefab", MessageType.Info);
                return;
            }

            foreach (var item in results)
            {
                EditorGUILayout.ObjectField(item.gameObject, typeof(GameObject), false);
            }
        }

        List<GameObject> GetItemsUseMaterial(Material mat)
        {
            return EditorTools.FindAssetsInProject<GameObject>("t:Prefab").Where(go =>
            {
                var r = go.GetComponent<Renderer>();
                return r && r.sharedMaterials.Contains(mat);
            }).ToList();
        }
    }
#endif
}