#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.WSA;
namespace PowerUtilities
{
    public static class EditorTextureTools 
    {
        public static void SaveTexturesToFolder(List<Texture2D> splitTextureList, string folder, int countInRow, bool showPropressBar = true)
        {
            for (int i = 0; i < splitTextureList.Count; i++)
            {
                var row = i / countInRow;
                var col = i % countInRow;

                var tex = splitTextureList[i];
                if (!tex)
                    continue;

                if (showPropressBar)
                    EditorUtility.DisplayProgressBar("SaveTextures", "Save Splited Textures", i);

                var bytes = tex.EncodeToPNG();
                File.WriteAllBytes(string.Format("{0}/{1}_{2}.png", folder, col, row), bytes);
            }

            if (showPropressBar)
                EditorUtility.ClearProgressBar();
        }

        public static void SaveTexturesDialog(List<Texture2D> textures, int countInRow, string dialogTitle = "Save SplitedTextures")
        {
            if (textures == null)
                return;

            var folder = EditorUtility.SaveFolderPanel(dialogTitle, "", "");
            if (string.IsNullOrEmpty(folder))
                return;

            SaveTexturesToFolder(textures, folder, countInRow);
        }

        public static void Create2DArray(List<Texture2D> textures,string assetPath,bool isLinear = true)
        {
            if (textures == null || textures.Count == 0)
                return;

            var arr = TextureTools.Create2DArray(textures, isLinear);
            AssetDatabase.CreateAsset(arr, assetPath);
        }

        [MenuItem(nameof(PowerUtilities) + "/CreateTexture/Empty2DArray")]
        public static void CreateEmptyTexture2DArray()
        {
            var tex = new Texture2DArray(64, 64, 2, TextureFormat.RGBA32, true);
            var colors = new Color32[64*64];
            colors.ForEach((c, id) =>
            {
                colors[id] = Color.white;
            });
            tex.SetPixelData(colors, 0,0);
            tex.SetPixelData(colors, 0,1);
            tex.Apply(false);

            var folder = SelectionTools.GetSelectedFolder(true);
            var path = $"{folder}/EmptyTexArr.asset";
            Selection.activeObject = AssetDatabaseTools.CreateAssetThenLoad<Texture2DArray>(tex, path);
        }

        [MenuItem(nameof(PowerUtilities) + "/CreateTexture/Empty3D")]
        public static void CreateEmptyTexture3D()
        {
            var tex = new Texture3D(32, 32, 2, TextureFormat.ASTC_6x6, true);
            var colors = Enumerable.Repeat(1, tex.width * tex.height).Select(num => Color.white).ToArray();
            tex.SetPixelData(colors, 0, 0);
            tex.SetPixelData(colors, 0, 1);
            tex.Apply();

            var folder = SelectionTools.GetSelectedFolder(true);
            var path = $"{folder}/EmptyTex3D.asset";
            Selection.activeObject = AssetDatabaseTools.CreateAssetThenLoad<Texture3D>(tex, path);
        }
        [MenuItem(nameof(PowerUtilities) + "/CreateTexture/Test")]
        public static void Test()
        {
            Texture2DArray tex;
            tex = new Texture2DArray(2, 2, 3, TextureFormat.RGBA32, true);
            var data = new byte[]
            {
            // mip 0: 2x2 size
            255, 0, 0,255, // red
            0, 255, 0,255, // green
            0, 0, 255,255, // blue
            255, 235, 4,255, // yellow
            // mip 1: 1x1 size
            0, 255, 255,255 // cyan
            };
            tex.SetPixelData(data, 0, 0, 0); // mip 0
            //tex.SetPixelData(data, 1, 0, 12); // mip 1
            tex.filterMode = FilterMode.Point;
            tex.Apply(updateMipmaps: false);

            var folder = SelectionTools.GetSelectedFolder(true);
            var path = $"{folder}/EmptyTex3D.asset";
            Selection.activeObject = AssetDatabaseTools.CreateAssetThenLoad<Texture3D>(tex, path);
        }
    }
}
#endif