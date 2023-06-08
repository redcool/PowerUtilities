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
            var count = 0;

            updaters.ForEach(updater => {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(updater.gameObject);
                
                PrefabTools.ModifyPrefab(prefab, (go) =>
                {
                    var updaters = go.GetComponentsInChildren<BaseUIMaterialPropUpdater>();
                    updaters.ForEach(updater =>
                    {
                        if (updater && updater.RestoreGraphMatFromShared())
                            count++;
                    });
                });
            });
            Debug.Log($"RestoreGraphMatFromSharedMats,prefabs{updaters.Length}, modified : {count}");
        }
    }
}
#endif