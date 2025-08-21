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

        [MenuItem(ASSET_BUNDLE_UTILS + "/BuildBundles")]
        static void BuildBundles()
        {
            CreateDir(BUNDLE_PATH);
            var target = EditorUserBuildSettings.activeBuildTarget;
            BuildPipeline.BuildAssetBundles(BUNDLE_PATH, BuildAssetBundleOptions.None, target);
        }

        private static void CreateDir(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        [MenuItem(ASSET_BUNDLE_UTILS + "/BuildSelected")]
        static void BuildSelected()
        {
            var obj = Selection.activeObject;
            if (!obj)
                return;
            var target = EditorUserBuildSettings.activeBuildTarget;
            AssetDatabaseTools.BuildObjectsAssetBundle(obj.name, buildTarget: target, bundlePath: BUNDLE_PATH, objects: Selection.objects);
        }
    }
}
#endif