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

        /// <summary>
        /// Create tex Array and fill with textures,
        /// texture array info get from textures[0]
        /// 
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="assetPath"></param>
        /// <param name="isLinear"></param>
        public static void Create2DArray(List<Texture2D> textures,string assetPath,bool isLinear = true)
        {
            if (textures == null || textures.Count == 0)
                return;

            var arr = TextureTools.Create2DArray(textures, isLinear);
            AssetDatabase.CreateAsset(arr, assetPath);
        }

        public static void Create3D(List<Texture2D> textures, string assetPath, bool isLinear = true)
        {
            if (textures == null || textures.Count == 0)
                return;

            var tex = TextureTools.Create3D(textures, isLinear);
            AssetDatabase.CreateAsset(tex, assetPath);
        }
    }
}
#endif