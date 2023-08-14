#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace PowerUtilities
{
    public class EditorTextureTools : MonoBehaviour
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

        public static void Create2DArray(List<Texture2D> textures,string assetPath)
        {
            var arr = TextureTools.Create2DArray(textures,false);
            AssetDatabase.CreateAsset(arr, assetPath);
        }
    }
}
#endif