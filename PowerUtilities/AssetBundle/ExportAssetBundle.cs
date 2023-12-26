namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEngine;
    using System.Collections;
    using UnityEditor;

    public class ExportAssetBundle : MonoBehaviour
    {
        public const string ASSET_BUNDLE_UTILS = "PowerUtilities/AssetBundleUtils";
        [MenuItem(ASSET_BUNDLE_UTILS + "/BuildBundles")]
        static void BuildBundles()
        {
            BuildPipeline.BuildAssetBundles("Assets/../Bundles", BuildAssetBundleOptions.None, BuildTarget.Android);
        }

        [MenuItem(ASSET_BUNDLE_UTILS+"/BuildSelected")]
        static void BuildSelected()
        {
            var obj = Selection.activeObject;
            if (!obj)
                return;

            var path = AssetDatabase.GetAssetPath(obj);
            var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
            imp.assetBundleName = obj.name;
            imp.SaveAndReimport();

            AssetDatabaseTools.SaveRefresh();

            var buildMap = new[]{
                new AssetBundleBuild()
                {
                    assetBundleName = obj.name,
                    assetNames = new[] {
                        path
                    }
                }
                };

            BuildPipeline.BuildAssetBundles("Assets/../Bundles", buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
        }
    }
#endif
}