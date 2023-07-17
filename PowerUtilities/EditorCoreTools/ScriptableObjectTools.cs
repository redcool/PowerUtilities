#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;


    public static class ScriptableObjectTools
    {
        /// <summary>
        /// get or create instance 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="so"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T GetInstance<T>(string path) where T : ScriptableObject
        {
            var settings = AssetDatabase.LoadAssetAtPath<T>(path);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        /// <summary>
        /// Get scriptable's serializedObject 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="so"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SerializedObject GetSerializedInstance<T>(string path) where T : ScriptableObject
        {
            return new SerializedObject(GetInstance<T>(path));
        }
    }
}
#endif