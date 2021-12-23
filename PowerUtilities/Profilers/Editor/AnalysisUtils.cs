namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using System.IO;
    using PowerUtilities;

    public class AnalysisUtils
    {
        public const string ANALYSIS_UTILS = "PowerUtilities/分析工具";
        [MenuItem(ANALYSIS_UTILS + "/ShowMaterials")]
        static void Init()
        {
            var mats = GetSceneMaterials(GetSceneGameObjects());
            ShowMaterials(mats);
        }

        [MenuItem(ANALYSIS_UTILS + "/ShowExcessObjects")]
        static void ShowExcess()
        {
            ShowExcessObjects(GetSceneGameObjects());
        }

        public static GameObject[] GetSceneGameObjects()
        {
            var q = from go in Resources.FindObjectsOfTypeAll<GameObject>()
                    where go.hideFlags != HideFlags.NotEditable
                    && go.hideFlags != HideFlags.HideAndDontSave
                    //&& EditorUtility.IsPersistent(r.transform.root.gameObject)
                    select go;
            return q.ToArray();
        }


        public static Material[] GetSceneMaterials(GameObject[] gos)
        {
            var matSet = new HashSet<Material>();
            foreach (var go in gos)
            {
                var rs = go.GetComponentsInChildren<Renderer>();
                foreach (var r in rs)
                {
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m)
                            matSet.Add(m);
                    }
                }
            }
            var mats = new Material[matSet.Count];
            matSet.CopyTo(mats);
            return mats;
        }

        public static void ShowMaterials(Material[] mats)
        {
            var shaderSet = new HashSet<Shader>();
            foreach (var m in mats)
            {
                shaderSet.Add(m.shader);
            }
            OutputMats(mats);
            OutputShaders(shaderSet);
        }

        public static void ShowExcessObjects(GameObject[] gos)
        {
            var excessSb = new StringBuilder();

            foreach (var go in gos)
            {
                var rs = go.GetComponentsInChildren<Renderer>();
                foreach (var r in rs)
                {
                    var mats = r.sharedMaterials;
                    foreach (var m in mats)
                    {
                        var shaderPath = AssetDatabase.GetAssetPath(m.shader);
                        if (!shaderPath.StartsWith("Assets"))
                        {
                            //Debug.Log(r +" -> "+ shaderPath);
                            excessSb.AppendFormat("{0} -> {1} \n", go, shaderPath);
                        }
                    }
                }
            }

            if (excessSb.Length != 0)
            {
                Debug.LogWarning(excessSb);
            }
            else
            {
                Debug.Log("not has excess objects.");
            }
        }

        static void OutputShaders(HashSet<Shader> set)
        {
            var arr = new Shader[set.Count];
            set.CopyTo(arr);

            var sb = new StringBuilder();
            sb.AppendLine("Shader count:" + arr.Length);
            foreach (var item in arr)
            {
                sb.AppendLine(AssetDatabase.GetAssetPath(item));
            }
            Debug.Log(sb);
        }

        static void OutputMats(Material[] mats)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Material count: " + mats.Length);
            foreach (var m in mats)
            {
                sb.AppendFormat("{0} -> shader:{1}\n", AssetDatabase.GetAssetPath(m), m.shader);
            }
            Debug.Log(sb);

            if (EditorUtility.DisplayDialog("Warning.", "Show In Hierarchy?", "ok"))
                Selection.activeGameObject = ShowMaterialsInInspector(mats);

            //new ChangeTexture().Change(mats);
            new ResetTexture().Reset(mats);
        }

        static GameObject ShowMaterialsInInspector(Material[] mats)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Material Results";
            var mr = go.GetComponent<MeshRenderer>();
            if (mr)
                mr.materials = mats;
            return go;
        }
    }

#endif
}