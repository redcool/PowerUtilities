#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System.IO;

    public static class SelectionTools
    {
        public static T[] GetSelectedComponents<T>() where T : Component
        {
            return Selection.gameObjects
                .Select(go => go.GetComponent<T>())
                .Where(t => t)
                .ToArray();
        }

        public static T[] GetSelectedChildrenComponents<T>(bool includeInactive=false) where T : Component
        {
            return Selection.gameObjects.
                SelectMany(go => go.GetComponentsInChildren<T>(includeInactive), (go, comp) => comp)
                .ToArray();
        }

        /// <summary>
        /// Get selected objects's folder paths
        /// </summary>
        /// <returns></returns>
        public static string[] GetSelectedFolders()
        {
            return Selection.objects
                .Select(AssetDatabaseTools.GetAssetFolder)
                .Where(str => !string.IsNullOrEmpty(str))
                .ToArray();
        }
    }
}
#endif