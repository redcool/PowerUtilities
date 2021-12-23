namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEngine;
    using System.Collections;
    using UnityEditor;

    public class ResetTexture
    {

        [MenuItem(AnalysisUtils.ANALYSIS_UTILS + "/ResetMaterials")]
        static void Init()
        {
            var mats = AnalysisUtils.GetSceneMaterials(AnalysisUtils.GetSceneGameObjects());
            new ResetTexture().Reset(mats);
        }

        public void Reset(Material[] mats)
        {
            foreach (var item in mats)
            {
                Reset(item);
            }
        }

        void Reset(Material mat)
        {
            Reset(mat, "_MainTex", "_rgb");
            Reset(mat, "_NormTexture", "_rgb");
            //Reset(mat, "_ExtraTex","_extra");
            mat.SetTexture("_ExtraTex", null);
        }

        void Reset(Material mat, string propName, string suffixName)
        {
            if (!mat.HasProperty(propName))
                return;

            var tex = mat.GetTexture(propName);
            if (!tex || !tex.name.EndsWith(suffixName))
                return;

            var path = AssetDatabase.GetAssetPath(tex);
            path = path.Substring(0, path.LastIndexOf(suffixName));
            path += ".tga";
            var originalTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            mat.SetTexture(propName, originalTex);
        }
    }
#endif
}