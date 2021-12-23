namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    public class ShaderListInfo
    {
        public List<ShaderAnalysis.ShaderMaterials> shaderList;
        public bool isFold;
        public string path;
    }

    public class ShaderAnalysisWindow : EditorWindow
    {
        List<ShaderListInfo> allShaderList;
        string[] selectedPaths;

        static bool needUpdate;

        private Vector2 scrollPos;
        int shaderCount;

        [MenuItem(ShaderAnalysis.SHADER_ANALYSIS+"/shader分析窗口",priority =1)]
        static void Init()
        {
            var win = GetWindow<ShaderAnalysisWindow>();
            win.Show();
            needUpdate = true;
        }

        private void Update()
        {
            if (needUpdate)
            {
                needUpdate = false;
                selectedPaths = EditorTools.GetSelectedObjectAssetFolder();
                allShaderList = FindAllShaders(selectedPaths,out shaderCount);
            }

        }

        private List<ShaderListInfo> FindAllShaders(string[] paths,out int shaderCount)
        {
            var list= new List<ShaderListInfo>();
            var count = 0;

            paths.ForEach(path =>
            {
                var shaderList = ShaderAnalysis.GetShaderInfos(path).ToList();
                list.Add(new ShaderListInfo {
                    shaderList = shaderList,
                    path = path
                });
                count += shaderList.Count;
            });
            shaderCount = count;
            return list;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical("Box");
            {
                GUILayout.BeginHorizontal("Box");
                {
                    if (GUILayout.Button("分析shader"))
                    {
                        needUpdate = true;
                    }

                    EditorGUILayout.LabelField(string.Format("总shader数:{0}", shaderCount));

                    GUILayout.EndHorizontal();
                }

                scrollPos = GUILayout.BeginScrollView(scrollPos, "Box");
                DrawShaderList();
                GUILayout.EndScrollView();

                GUILayout.EndVertical();
            }
        }

        void DrawShaderList()
        {
            if(allShaderList == null || allShaderList.Count == 0)
            {
                EditorGUILayout.HelpBox("未选中要分析的目录",MessageType.Info,true);
                return;
            }

            allShaderList.ForEach(shaderListInfo =>
            {

                EditorGUILayout.LabelField(string.Format("{0},数量 : {1}", shaderListInfo.path, shaderListInfo.shaderList.Count));

                if (shaderListInfo.shaderList.Count <= 0)
                    return;

                shaderListInfo.isFold = EditorGUILayout.Foldout(shaderListInfo.isFold, "shader列表");
                if (shaderListInfo.isFold)
                {

                    EditorGUILayout.BeginVertical("Box");

                    shaderListInfo.shaderList.ForEach(item => DrawShaderInfo(item));

                    EditorGUILayout.EndVertical();
                }

            });
             
        }

        void DrawShaderInfo(ShaderAnalysis.ShaderMaterials item)
        {
            // shader name, ref counts
            EditorGUI.indentLevel = 0;
            var count = item.materials.Count();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.ObjectField(item.shader, typeof(Shader), false, GUILayout.MinWidth(50));

                if (count > 0)
                    item.isFold = EditorGUILayout.Foldout(item.isFold, "材质引用数 : " + count);

                if (item.refByCode)
                    GUILayout.Box("Code");

                EditorGUILayout.EndHorizontal();
            }

            // material objects
            if (item.isFold)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginVertical();
                {
                    foreach (var mat in item.materials)
                    {
                        EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MinWidth(100));
                    }

                    EditorGUILayout.EndVertical();
                }
            }

        }
    }
#endif
}