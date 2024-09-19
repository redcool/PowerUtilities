namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.IO;

    public class ExportAssetBundle : MonoBehaviour
    {
        public const string ASSET_BUNDLE_UTILS = "PowerUtilities/AssetBundleUtils";

        const string BUNDLE_PATH = "Assets/../Bundles";
        [MenuItem(ASSET_BUNDLE_UTILS + "/BuildBundles")]
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

        [MenuItem(ASSET_BUNDLE_UTILS+"/BuildSelected")]
        static void BuildSelected()
        {
            var obj = Selection.activeObject;
            if (!obj)
                return;

            var path = AssetDatabase.GetAssetPath(obj);
            var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
            
            if(string.IsNullOrEmpty(imp.assetBundleName))
                imp.assetBundleName = obj.name;

            imp.SaveAndReimport();

            AssetDatabaseTools.SaveRefresh();

            var buildMap = new[]{
                new AssetBundleBuild()
                {
                    assetBundleName = imp.assetBundleName,
                    assetNames = new[] {
                        path
                    }
                }
                };

            CreateDir(BUNDLE_PATH);
            BuildPipeline.BuildAssetBundles(BUNDLE_PATH, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
        }
    }
#endif
}