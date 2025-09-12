#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Presets;

namespace PowerUtilities
{
    public static class AssetImporterTools
    {
        /// <summary>
        /// Is subClass AssetImporter, execulde NativeFormatAssetImporter
        /// </summary>
        /// <param name="imp"></param>
        /// <returns></returns>
        public static bool IsAssetImporter(this AssetImporter imp)
        {
            return imp.GetType().IsSubclassOf(typeof(AssetImporter));
        }
    }
}
#endif