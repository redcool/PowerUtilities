#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{
    public class RestoreGraphMatFromSharedMats
    {

        [MenuItem(UIMaterialPropCodeGen.MENU_ROOT+"/Tools/"+nameof(RestoreGraphMatFromSharedMats))]
        static void Restore()
        {
            var folders = SelectionTools.GetSelectedFolders();
            var updaters = AssetDatabaseTools.FindComponentsInProject<BaseUIMaterialPropUpdater>("", folders);
            var q = AssetDatabaseTools.FindAssetsInProject<GameObject>("", folders)
                .Where(go => go.GetComponentInChildren<BaseUIMaterialPropUpdater>());

            q.ForEach(go =>
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);

                PrefabTools.ModifyPrefab(prefab, (go) =>
                {
                    /*
                    var updaters = go.GetComponentsInChildren<BaseUIMaterialPropUpdater>();
                    var outerPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                    updaters.ForEach(updater =>
                    {
                        var innerPrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(updater.gameObject);
                        if (innerPrefab == outerPrefab)
                        {
                            if (updater.RestoreGraphMatFromShared())
                                count++;
                        }
                    });
                    */
                });
            });
            Debug.Log($"RestoreGraphMatFromSharedMats,prefabs{q.Count()}");
        }
    }
}
#endif