#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// AssetBundle Info
    /// </summary>
    [Serializable]
    public class AssetBundleInfo
    {
        public string abName;
        /// <summary>
        /// assets in this bundle
        /// </summary>
        public List<string> assetPathList = new();
        /// <summary>
        /// dependencies of this bundle
        /// </summary>
        public List<List<string>> dependencyList = new();

        public int DependencyDepth => dependencyList.Count;

        public override string ToString()
        {
            return abName;
        }

        public void UpdateBundleAssets()
        {
            assetPathList.Clear();
            assetPathList.AddRange(AssetDatabase.GetAssetPathsFromAssetBundle(abName));
        }

        public void UpdateDependencies()
        {
            dependencyList.Clear();
            AssetDatabaseTools.FindAssetBundleDependenciesRecursive(abName, ref dependencyList);
        }


    }
}
#endif