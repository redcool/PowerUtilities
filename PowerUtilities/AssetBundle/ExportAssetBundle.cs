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
        static void Init()
        {
            BuildPipeline.BuildAssetBundles("Assets/../Bundles", BuildAssetBundleOptions.None, BuildTarget.Android);
        }
    }
#endif
}