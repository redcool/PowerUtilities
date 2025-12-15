#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class TextureCreateMenu
    {

        [MenuItem(ContextMenuConsts.POWER_UTILS_MENU + "/CreateTexture/Empty2DArray")]
        public static void CreateEmptyTexture2DArray()
        {
            var tex = new Texture2DArray(64, 64, 2, TextureFormat.RGBA32, true);
            var colors = new Color32[64 * 64];
            colors.ForEach((c, id) =>
            {
                colors[id] = Color.white;
            });
            tex.SetPixelData(colors, 0, 0);
            tex.SetPixelData(colors, 0, 1);
            tex.Apply(false);

            var folder = SelectionTools.GetSelectedFolder(true);
            var path = $"{folder}/EmptyTexArr.asset";
            Selection.activeObject = AssetDatabaseTools.CreateAssetThenLoad<Texture2DArray>(tex, path);
        }

        [MenuItem(ContextMenuConsts.POWER_UTILS_MENU + "/CreateTexture/Empty3D")]
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
        //[MenuItem(ContextMenuConsts.POWER_UTILS_MENU + "/CreateTexture/Test")]
        public static void TestCreateEmptyTex3D()
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