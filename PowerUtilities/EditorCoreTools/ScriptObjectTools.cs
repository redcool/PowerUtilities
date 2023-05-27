#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;


    public static class ScriptObjectTools
    {
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            PathTools.CreateAbsFolderPath(path);

            var t = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!t)
            {
                var newT = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(newT, path);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return t;
        }
    }
}
#endif