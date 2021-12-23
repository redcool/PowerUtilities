#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;
    using System.IO;
    using UnityEngine.Experimental.Rendering;

    public static class TextureTools
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
        /// Split Textures by resolution
        /// </summary>
        /// <param name="textures"></param>
        /// <param name="resolution"></param>
        /// <param name="countInRow"></param>
        /// <returns></returns>
        public static List<Texture2D> SplitTextures(Texture2D[] textures, TextureResolution resolution, ref int countInRow,Action<float> onProgress,bool isHeightmap, TextureFormat textureFormat= TextureFormat.R8, bool isMipChain=false, bool isLinear=true)
        {
            if (textures == null || textures.Length == 0)
                return null;

            var res = (int)resolution;
            var textureList = new List<Texture2D>();
            countInRow = 0;

            for (int i = 0; i < textures.Length; i++)
            {
                var tex = textures[i];
                if (tex == null)
                    continue;


                var countInRowTile = tex.width / res;
                countInRowTile = Mathf.Max(1, countInRowTile);

                countInRow += countInRowTile;

                if (tex.width > res)
                {
                    var texs = tex.SplitTexture(res,onProgress,isHeightmap,textureFormat,isMipChain,isLinear);
                    textureList.AddRange(texs);
                }
                else
                {
                    textureList.Add(tex);
                }
            }
            return textureList;
        }

    }
}
#endif
