namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;
    using UnityEngine.Networking;
    using System.Collections.Generic;
    using System;
    using Object = UnityEngine.Object;

    public static class BundleLoader
    {
        public static Dictionary<string, AssetBundle> bundleDict = new Dictionary<string, AssetBundle>();

        public static IEnumerator WaitForLoadBundle(string path, Action<AssetBundle> onLoaded)
        {
            if (onLoaded == null)
                yield break;

            using (var req = UnityWebRequestAssetBundle.GetAssetBundle(path))
            {
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(req);
                    bundleDict[path] = bundle;

                    onLoaded(bundle);
                }
            }
        }

        public static IEnumerator WaitForLoadManifestFromBundle(string path, Action<AssetBundleManifest> onLoaded)
        {
            yield return WaitForLoadBundle(path, (ab) =>{
                var mani = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                onLoaded(mani);
            });

        }
        public static IEnumerator WaitForLoadFromBundle<T>(string path, string assetName, Action<T> onLoaded) where T : Object
        {
            if (onLoaded == null)
                yield break;

            AssetBundle ab;
            if (bundleDict.TryGetValue(path, out ab))
            {
                onLoaded(ab.LoadAsset<T>(assetName));
                yield break;
            }

            yield return WaitForLoadBundle(path, (ab =>
            {
                onLoaded(ab.LoadAsset<T>(assetName));
            }));

        }
    }
}