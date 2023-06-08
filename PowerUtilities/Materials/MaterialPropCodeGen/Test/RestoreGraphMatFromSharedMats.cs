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
            var imagePrefabs = AssetDatabaseTools.FindComponentsInProject<Image>("", folders);
            var count = 0;

            imagePrefabs.ForEach(image => {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(image.gameObject);
                
                PrefabTools.ModifyPrefab(prefab, (go) =>
                {
                    var updaters = go.GetComponentsInChildren<BaseUIMaterialPropUpdater>();
                    var images = go.GetComponentsInChildren<Image>();
                    images.ForEach(image =>
                    {
                        var updater = image.GetComponent<BaseUIMaterialPropUpdater>();
                        if((bool)(updater?.RestoreGraphMatFromShared()))
                            count++;
                    });
                });
            });
            Debug.Log($"RestoreGraphMatFromSharedMats done : {count}");
        }
    }
}
#endif