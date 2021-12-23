namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using System.Collections;
    using System.IO;

    public class ChangeTexture
    {
        const string RGB_PNG = "_rgb.png";
        const string EXTRA_PNG = "_extra.png";
        const string PROP_EXTRA_TEX = "_ExtraTex";

        [MenuItem(AnalysisUtils.ANALYSIS_UTILS + "/ChangeTextures")]
        static void Init()
        {
            var mats = AnalysisUtils.GetSceneMaterials(AnalysisUtils.GetSceneGameObjects());
            new ChangeTexture().Change(mats);
        }

        string DataPath
        {
            get
            {
                var d = Application.dataPath;
                return d.Substring(0, d.LastIndexOf("Assets"));
            }
        }

        public void Change(Material[] mats)
        {
            foreach (var item in mats)
            {
                Change(item);
            }
            Debug.Log("Change done.");
        }

        void Change(Material mat)
        {
            UpdateMaterial(mat, "_MainTex");
            UpdateMaterial(mat, "_NormTexture");

            CreateExtraMap(mat);
        }

        void UpdateMaterial(Material mat, string propName)
        {
            if (!mat.HasProperty(propName))
            {
                Debug.Log(mat.name + " not has " + propName);
                return;
            }
            var tex = mat.GetTexture(propName) as Texture2D;
            if (tex)
            {
                if (!tex.name.EndsWith("_rgb"))
                {
                    var ft = tex.format.ToString();
                    if (ft.Contains("RGBA"))
                    {
                        var newTex = GetOrCreateTexture(tex as Texture2D);
                        mat.SetTexture(propName, newTex);
                    }
                }
            }
        }


        Texture2D GetOriginalTex(Texture2D texRGB)
        {
            if (!texRGB)
                return null;

            var originalTexPath = AssetDatabase.GetAssetPath(texRGB);
            originalTexPath = originalTexPath.Substring(0, originalTexPath.LastIndexOf("_rgb")) + ".tga";
            return AssetDatabase.LoadAssetAtPath<Texture2D>(originalTexPath);
        }

        void CreateExtraMap(Material mat)
        {

            if (!mat.HasProperty(PROP_EXTRA_TEX))
                return;

            var extraMap = mat.GetTexture(PROP_EXTRA_TEX) as Texture2D;

            if (!extraMap)
            {
                var mainTexRGB = mat.GetTexture("_MainTex") as Texture2D;
                if (!mainTexRGB || !mainTexRGB.name.EndsWith("_rgb"))
                    return;

                var mainTex = GetOriginalTex(mainTexRGB);
                if (!mainTex)
                    return;

                var normTexRGB = mat.GetTexture("_NormTexture") as Texture2D;
                var normTex = GetOriginalTex(normTexRGB);


                extraMap = new Texture2D(mainTex.width, mainTex.height, TextureFormat.RGB24, true);

                for (int x = 0; x < mainTex.width; x++)
                {
                    for (int y = 0; y < mainTex.height; y++)
                    {
                        var c = new Color();
                        c.r = mainTex.GetPixel(x, y).a;
                        if (normTex)
                            c.g = normTex.GetPixel(x, y).a;
                        extraMap.SetPixel(x, y, c);
                    }
                }

                extraMap.Apply(true);

                var extraMapAssetPath = GetTexAssetPath(mainTex, EXTRA_PNG);
                File.WriteAllBytes(DataPath + extraMapAssetPath, extraMap.EncodeToPNG());
                AssetDatabase.Refresh();

                extraMap = AssetDatabase.LoadAssetAtPath<Texture2D>(extraMapAssetPath);
                mat.SetTexture(PROP_EXTRA_TEX, extraMap);
            }
        }

        void MakeReadable(Texture2D tex)
        {
            if (!tex)
                return;

            var texPath = AssetDatabase.GetAssetPath(tex);
            var imp = TextureImporter.GetAtPath(texPath) as TextureImporter;
            imp.isReadable = true;
            imp.SaveAndReimport();
        }

        string GetTexAssetPath(Texture2D tex, string suffix)
        {
            var texPath = AssetDatabase.GetAssetPath(tex);
            var dir = Path.GetDirectoryName(texPath);
            return dir + "/" + tex.name + suffix;//"_rgb.png";
        }
        Texture2D GetOrCreateTexture(Texture2D tex)
        {
            var texRGBAssetPath = GetTexAssetPath(tex, RGB_PNG);
            var texRGB = AssetDatabase.LoadAssetAtPath<Texture2D>(texRGBAssetPath);

            if (!texRGB)
            {
                MakeReadable(tex);
                var newTex = new Texture2D(tex.width, tex.height);
                for (int x = 0; x < tex.width; x++)
                {
                    for (int y = 0; y < tex.height; y++)
                    {
                        var c = tex.GetPixel(x, y);
                        newTex.SetPixel(x, y, new Color(c.r, c.g, c.b));
                    }
                }
                newTex.Apply(true);

                File.WriteAllBytes(DataPath + texRGBAssetPath, newTex.EncodeToPNG());
                AssetDatabase.Refresh();
            }

            texRGB = AssetDatabase.LoadAssetAtPath<Texture2D>(texRGBAssetPath);
            return texRGB;
        }
    }
}