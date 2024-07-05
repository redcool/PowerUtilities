#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// register assetImport callback 
    /// </summary>
    public class AssetPostProcessorControl : AssetPostprocessor
    {
        public static event Action<AssetImporter> onPreprocessAsset;

        public static event Action<AssetImporter,Object> onPostProcessAsset;

        [InitializeOnLoadMethod]
        static void OnInit()
        {
            ApplicationTools.OnDomainUnload += () =>
            {
                onPreprocessAsset = null;
                onPostProcessAsset = null;
            };
        }

        void OnPreprocessAsset()
        {
            onPreprocessAsset?.Invoke(assetImporter);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (onPostProcessAsset != null)
            {

                foreach (string path in importedAssets)
                {
                    var imp = AssetImporter.GetAtPath(path);
                    var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                    onPostProcessAsset?.Invoke(imp,asset);
                }
            }
        }

    }
}
#endif