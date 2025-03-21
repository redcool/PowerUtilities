#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.IO;

    public class ExportAssetBundle
    {
        public const string ASSET_BUNDLE_UTILS = "PowerUtilities/AssetBundleUtils";
        const string BUNDLE_PATH = "Assets/../Bundles";

        [MenuItem(ASSET_BUNDLE_UTILS + "/BuildBundles (android)")]
        static void BuildBundles()
        {
            CreateDir(BUNDLE_PATH);

            BuildPipeline.BuildAssetBundles(BUNDLE_PATH, BuildAssetBundleOptions.None, BuildTarget.Android);
        }

        private static void CreateDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        [MenuItem(ASSET_BUNDLE_UTILS + "/BuildSelected (android)")]
        static void BuildSelected()
        {
            var obj = Selection.activeObject;
            if (!obj)
                return;

            AssetDatabaseTools.BuildObjectsAssetBundle(obj.name, buildTarget: BuildTarget.Android, bundlePath: BUNDLE_PATH, objects: Selection.objects);
        }
    }
}
#endif