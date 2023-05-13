namespace PowerUtilities
{
#if UNITY_EDITOR
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Text;
    using System.IO;

    public class ShaderAnalysis
    {
        public class ShaderMaterials
        {
            public Shader shader;
            public List<Material> materials;
            public bool isFold;
            public bool refByCode;
        }

        public const string SHADER_ANALYSIS = "PowerUtilities/分析工具/Shader分析";

        [MenuItem(SHADER_ANALYSIS + "/分析Project")]
        static void AnslysisAllShaders()
        {
            var sb = new StringBuilder();
            // search path

            var q = GetShaderInfos();
            q.Select(item =>
            {
                var refByCode = item.refByCode && item.materials.Count == 0 ? ",Ref by Code" : "";
                return string.Format("{0,-100} => count: {1} {2} \n", item.shader.name, item.materials.Count(), refByCode);
            }).ForEach(item => sb.Append(item));

            sb.AppendLine();

            //output to console
            Debug.Log(sb);

            //output to file
            var folder = EditorUtility.OpenFolderPanel("保存shader分析", "", "shader分析报告");
            if (!string.IsNullOrEmpty(folder))
            {
                File.WriteAllText(folder + "/shander分析报告.txt", sb.ToString());
            }
        }

        [MenuItem(SHADER_ANALYSIS + "/分析选中的目录")]
        static void AnslysisSelectedFolder()
        {
            var paths = EditorTools.GetSelectedObjectAssetFolder();
            if (paths.Count() == 0)
            {
                Debug.Log("未选中要分析的目录");
                return;
            }

            var sb = new StringBuilder();
            paths.ForEach(path =>
            {
                // add title
                sb.AppendLine(path).AppendLine();
                // search path

                var q = GetShaderInfos(path);
                q.Select(item => string.Format("{0,-100} => {1} \n", item.shader.name, item.materials.Count()))
                    .ForEach(item => sb.Append(item));

                sb.AppendLine();
            });

            //output to console
            Debug.Log(sb);

            //output to file
            var folder = EditorUtility.OpenFolderPanel("保存shader分析", "", "shader分析报告");
            if (!string.IsNullOrEmpty(folder))
            {
                File.WriteAllText(folder + "/shander分析报告.txt", sb.ToString());
            }
        }

        [MenuItem(SHADER_ANALYSIS + "/选择未使用的shader")]
        static void SelectUnusedShaders()
        {
            var list = new List<Shader>();
            var paths = EditorTools.GetSelectedObjectAssetFolder();
            if (paths.Length == 0)
            {
                Debug.Log("需要选中目标目录");
                return;
            }

            paths.ForEach(Path =>
            {

                var q = GetShaderInfos();
                var unusedQueue = q.Where(sm => sm.materials.Count() == 0 && !sm.refByCode)
                    .Select(sm => sm.shader).ToList();

                list.AddRange(unusedQueue);
            });


            Selection.objects = list.ToArray();
        }
        /*
        [MenuItem(SHADER_ANALYSIS + "/移除未使用的shader")]
        static void RemoveUnusedShaders()
        {
            var list = new List<string>();
            var paths = EditorTools.GetSelectedObjectAssetFolder();
            if (paths.Length == 0)
            {
                Debug.Log("需要选中目标目录");
                return;
            }

            paths.ForEach(Path=> {
                var q = GetShaderInfos();
                var unusedQueue = q.Where(sm => sm.materials.Count() == 0 && !sm.refByCode)
                    .Select(sm => sm.shader)
                    .Select(shader => AssetDatabase.GetAssetPath(shader)).ToList();

                list.AddRange(unusedQueue);
            });

            Selection.objects = null;
            if(EditorUtility.DisplayDialog("Warning!","Deleted all unused shaders?", "ok"))
            {
                list.ForEach(path => AssetDatabase.MoveAssetToTrash(path));
            }
        }

        */
        public static IEnumerable<ShaderMaterials> GetShaderInfos(params string[] folders)
        {
            var shaders = AssetDatabaseTools.FindAssetsInProject<Shader>("",folders);
            var mats = AssetDatabaseTools.FindAssetsInProject<Material>();

            var q = shaders.Select(shader => new ShaderMaterials
            {
                shader = shader,
                materials = mats.Where(mat => mat.shader == shader).ToList(),
                refByCode = ShaderExclude.IsRefByCode(shader)
            }).OrderBy(sm => sm.materials.Count());
            return q;
        }

        public static IEnumerable<Material> GetShaderInfo(Shader shader)
        {
            if (!shader)
                return null;

            var mats = AssetDatabaseTools.FindAssetsInProject<Material>();
            return mats.Where(mat => mat.shader == shader);
        }
    }
#endif
}