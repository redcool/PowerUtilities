#if UNITY_EDITOR && NGUI
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using System.IO;
    using System;

    /// <summary>
    /// 分离rgba32为rgb图,r图,alpha8图
    /// 分离后的图片使用压缩格式.
    /// 
    /// 操作:
    /// 1 选中要分离通道的目录或图片. 点击 PowerUtilities/TextureTools/Split Selected Textures
    /// 2 旋转要更新atlasPrefab的目录或prefabs,点击 PowerUtilities/TextureTools/Update Selected NGUI Atalses
    /// 
    /// android : rgb(etc) , alpha8
    /// ios : rgb , r (pvrtc)
    /// </summary>
    public static class TextureChannelSplit
    {
        const string ARG_AlphaTexChannel = "_AlphaTexChannel";
        const string KEY_ALPHATEXCHANNEL_NO = "_ALPHATEXCHANNEL_NO";
        const string KEY_ALPHATEXCHANNEL_R = "_ALPHATEXCHANNEL_R";
        const string KEY_ALPHATEXCHANNEL_A = "_ALPHATEXCHANNEL_A";

        const string NAME_MAIN_TEX = "_MainTex";
        const string NAME_ALPHA_TEX = "_AlphaTex";

        const string TAG_R = "_r";
        const string TAG_ALPHA = "_alpha";
        const string TAG_RGB = "_rgb";
        const string TEX_EXTNAME = ".png";

        [MenuItem("PowerUtilities/TextureTools/Split Selected Textures")]
        static void SplitSelectedTexutes()
        {
            var texs = EditorTools.GetFilteredFromSelection<Texture2D>(SelectionMode.Assets | SelectionMode.DeepAssets);
            SplitTextures(texs);
        }

        [MenuItem("PowerUtilities/TextureTools/Update Selected NGUI Atalses")]
        static void UpdateSelectedAtals()
        {
            // get all uiAtlas
            var gos = EditorTools.GetFilteredFromSelection<GameObject>(SelectionMode.Assets | SelectionMode.DeepAssets);
            var q = gos.Where(go => go.GetComponent<UIAtlas>())
                .Select(go => go.GetComponent<UIAtlas>());
            UpdateAtlases(q.ToArray());
        }

        [MenuItem("PowerUtilities/TextureTools/Restore Atlas(use rgba)")]
        static void RestoreAtals()
        {
            var gos = EditorTools.GetFilteredFromSelection<GameObject>(SelectionMode.Assets | SelectionMode.DeepAssets);
            var q = gos.Where(go => go.GetComponent<UIAtlas>())
                .Select(go => go.GetComponent<UIAtlas>());
            RestoreAtlases(q.ToArray());
        }

        private static void RestoreAtlases(UIAtlas[] items)
        {
            foreach (var item in items)
            {
                var path = GetAtlasPrefabPath(item);
                var mainTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path + TEX_EXTNAME);
                var mat = item.spriteMaterial;
                mat.SetTexture(NAME_MAIN_TEX, mainTex);
                mat.SetTexture(NAME_ALPHA_TEX,null);
                mat.SetFloat(ARG_AlphaTexChannel, 0);

                mat.DisableKeyword(KEY_ALPHATEXCHANNEL_A);
                mat.DisableKeyword(KEY_ALPHATEXCHANNEL_R);
                mat.DisableKeyword(KEY_ALPHATEXCHANNEL_NO);
            }
            Debug.Log("UpdateSelectedAtals done. " + items.Length);
        }

        public static void SplitTextureUpdateAtlas(params string[] assetPaths)
        {
            SplitSelectedTexutes(assetPaths);
            UpdateAtlasInPath(assetPaths);
        }
        /// <summary>
        ///  更新assetPaths的UIAtlas材质并使用分离通道的图
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void UpdateAtlasInPath(params string[] assetPaths)
        {
            var items = EditorTools.FindComponentFromAssets<UIAtlas>("t:GameObject", assetPaths);

            UpdateAtlases(items);
        }
        static void UpdateAtlases(UIAtlas[] items)
        {
            // get uiAtlas materials
            foreach (var item in items)
            {
                UpdateAtlasMaterial(item);
            }
            Debug.Log("UpdateSelectedAtals done. " + items.Length);
        }

        /// <summary>
        /// 对指定目录下的texture2D分离通道
        /// </summary>
        /// <param name="assetPaths"></param>
        public static void SplitSelectedTexutes(params string[] assetPaths)
        {
            var items = EditorTools.FindAssetsInProject<Texture2D>("t:Texture2D", assetPaths);
            SplitTextures(items);
        }

        static void SplitTextures(Texture2D[] texs)
        {
            var q = texs.Where(t => !(t.name.EndsWith(TAG_R) || t.name.EndsWith(TAG_RGB) || t.name.EndsWith(TAG_ALPHA)));

            foreach (var tex in q)
            {
                SplitTexture(tex);
            }
        }
        static void SplitTexture(Texture2D tex)
        {
            Texture2D rgbTex;
            Texture2D alphaTex;
            //1 setttings
            tex.Setting(imp => {
                imp.isReadable = true;
                imp.SetPlatformTextureSettings(new TextureImporterPlatformSettings
                {
                    format = TextureImporterFormat.RGBA32,
                });
                imp.SaveAndReimport();
            });
            //2 split channels
            SplitRGBA(tex, out rgbTex, out alphaTex);

            //3 save rgb,a
            var path = AssetDatabase.GetAssetPath(tex);
            //var dir = Path.GetDirectoryName(path);
            //dir += "/" + tex.name;
            var dir = PathTools.GetAssetDir(path, "/", tex.name);
            var rgbTexPath = dir + "_rgb.png";
            var alphaTexPath = dir + "_r.png";

            Save(rgbTexPath, rgbTex);
            Save(alphaTexPath, alphaTex);
            var newAlphaPath = SaveAlphaTex(path);
            AssetDatabase.Refresh();

            //4 compress
            SetTextureFormat(newAlphaPath, rgbTexPath, alphaTexPath);
            //5
            AssetDatabase.Refresh();
        }

        static string GetAtlasPrefabPath(UIAtlas atlas)
        {
            var assetPath = AssetDatabase.GetAssetPath(atlas);
            var prefabName = Path.GetFileNameWithoutExtension(assetPath);
            var path = PathTools.GetAssetDir(assetPath, "/", prefabName);
            return path;
        }

        /// <summary>
        /// 根据atlas的name获取rgb图片
        /// 更新material
        /// </summary>
        /// <param name="atlas"></param>
        static void UpdateAtlasMaterial(UIAtlas atlas)
        {
            var prefabPath = GetAtlasPrefabPath(atlas);
            var rgbTex = AssetDatabase.LoadAssetAtPath<Texture2D>(prefabPath + "_rgb.png");
            var rTex = AssetDatabase.LoadAssetAtPath<Texture2D>(prefabPath + "_r.png");
            var alphaTex = AssetDatabase.LoadAssetAtPath<Texture2D>(prefabPath + "_alpha.png");

            var mat = atlas.spriteMaterial;
            //mat.shader = Shader.Find("Unlit/Transparent Colored (rgb+a)");
            mat.SetTexture(NAME_MAIN_TEX, rgbTex);
            mat.SetTexture(NAME_ALPHA_TEX, rTex);

#if UNITY_ANDROID
            // rgb + alpha
            mat.SetTexture(NAME_ALPHA_TEX, alphaTex);
            mat.EnableKeyword(KEY_ALPHATEXCHANNEL_A);
            mat.SetFloat(ARG_AlphaTexChannel, 2);
#else
            mat.EnableKeyword(KEY_ALPHATEXCHANNEL_R);
            mat.SetFloat(ARG_AlphaTexChannel, 1);
#endif
            if (!rgbTex)
                Debug.LogError("Cannot fount texture:" + prefabPath);
        }

        static void SplitRGBA(Texture2D tex, out Texture2D rgbTex, out Texture2D alphaTex)
        {
            var colors = tex.GetPixels();
            var rgbs = colors.Select((c) => new Color(c.r, c.g, c.b)).ToArray();
            var alphas = colors.Select(c => new Color(c.a, 0, 0)).ToArray();

            rgbTex = new Texture2D(tex.width, tex.height);
            alphaTex = new Texture2D(tex.width, tex.height);

            rgbTex.SetPixels(rgbs);
            rgbTex.Apply();

            alphaTex.SetPixels(alphas);
            alphaTex.Apply();
        }

        static void Save(string assetPath, Texture2D tex)
        {
            var absPath = PathTools.GetAssetAbsPath(assetPath);
            File.WriteAllBytes(absPath, tex.EncodeToPNG());
        }

        static string SaveAlphaTex(string path)
        {
            // new ui atlas 
            var absPath = PathTools.GetAssetAbsPath(path);
            var newFilePath = Path.GetDirectoryName(absPath) + "/" + Path.GetFileNameWithoutExtension(absPath) + "_alpha" + Path.GetExtension(absPath);
            File.Copy(absPath, newFilePath, true);
            return PathTools.GetAssetPath(newFilePath);
        }

        static void SetTextureFormat(string path, string rgbTexPath, string alphaTexPath)
        {
            // prepare settings
            var alphaTexSeting = new TextureImporterPlatformSettings
            {
                format = TextureImporterFormat.Alpha8
            };

            var rgbTexSeting = new TextureImporterPlatformSettings();
            rgbTexSeting.textureCompression = TextureImporterCompression.Compressed;

#if UNITY_ANDROID
            rgbTexSeting.format = TextureImporterFormat.ETC_RGB4;
#elif UNITY_IOS
            rgbTexSeting.format = TextureImporterFormat.PVRTC_RGB4;
#endif

            var setings = new[] { alphaTexSeting, rgbTexSeting, rgbTexSeting };
            var texPaths = new[] { path, rgbTexPath, alphaTexPath };

            for (int i = 0; i < setings.Length; i++)
            {
                AssetDatabase.LoadAssetAtPath<Texture2D>(PathTools.GetAssetPath(texPaths[i])).Setting(
                    imp =>
                    {
                        imp.SetPlatformTextureSettings(setings[i]);
                        imp.mipmapEnabled = false;
                        imp.SaveAndReimport();
                    }
                    );
            }
        }
    }
}
#endif