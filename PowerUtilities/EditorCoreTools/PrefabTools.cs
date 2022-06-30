namespace PowerUtilities
{
    using System;
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public static class PrefabTools
    {
        public static void RenamePrefab(GameObject instance,string newName)
        {
            if (!instance)
                return;

            // rename prefab'name
            var p = PrefabUtility.GetCorrespondingObjectFromOriginalSource(instance);
            if (p)
            {
                var path = AssetDatabase.GetAssetPath(p);
                AssetDatabase.RenameAsset(path, newName);
            }
        }


        public static void ModifyPrefab(GameObject prefab,Action<GameObject> onModify)
        {
            if (!prefab || onModify == null)
                return;

            //var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            //var prefabInst = PrefabUtility.LoadPrefabContents(path);

            (string path,GameObject prefabInst) = OpenPrefab(prefab);

            onModify(prefabInst);

            //PrefabUtility.SaveAsPrefabAsset(prefabInst, path);
            //PrefabUtility.UnloadPrefabContents(prefabInst);
            CloseSavePrefab(prefabInst, path);
        }

        public static (string prefabPath,GameObject prefabInst) OpenPrefab(GameObject prefab)
        {
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab);
            var prefabInst = PrefabUtility.LoadPrefabContents(path);
            return (path, prefabInst);
        }

        public static void CloseSavePrefab(GameObject prefabInst,string path)
        {
            PrefabUtility.SaveAsPrefabAsset(prefabInst, path);
            PrefabUtility.UnloadPrefabContents(prefabInst);
        }
    }
#endif
}