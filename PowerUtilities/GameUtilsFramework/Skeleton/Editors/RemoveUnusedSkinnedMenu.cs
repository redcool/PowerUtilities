#if UNITY_EDITOR
namespace GameUtilsFramework
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using PowerUtilities;
    using System.Linq;

    public class RemoveUnusedSkinnedMenu
    {
        [MenuItem(SaveSkinnedBonesWindow.MENU_PATH+"/RemoveUnusedSkinnedMesh(Hierchy_Project)")]
        static void RemoveSkinneds()
        {
            //if(!EditorUtility.DisplayDialog("waring","delete unused skinnedMeshs", "ok"))
            //    return;
            foreach (var item in Selection.gameObjects)
            {
                Object prefabInst = item;
                var isPrefabInProject = PrefabUtility.IsPartOfPrefabAsset(item);
                if (isPrefabInProject)
                {
                    prefabInst = PrefabUtility.InstantiatePrefab(item);
                }

                var prefab = PrefabUtility.GetOutermostPrefabInstanceRoot(prefabInst);
                PrefabTools.ModifyPrefab(prefab, go =>
                {
                    var q = go.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                    .Where(x => !x.gameObject.activeSelf);

                    foreach (var item in q)
                    {
                        Object.DestroyImmediate(item.gameObject);
                    }
                });
                if (isPrefabInProject)
                {
                    Object.DestroyImmediate(prefabInst);
                }
            }
        }
    }
}
#endif