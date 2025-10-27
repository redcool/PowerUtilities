#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class ScriptableObjectTools
    {
        static CacheTool<string,SerializedObject> cachedSerializedObjects = new CacheTool<string,SerializedObject>();

        /// <summary>
        /// Get or create instance 
        /// if path is empty will find from AssetPathAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="so"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Object CreateGetInstance(Type type,string path = "")
        {
            // check path from attribute
            if (string.IsNullOrEmpty(path))
            {
                path = SOAssetPathAttribute.GetPath(type);
                if (string.IsNullOrEmpty(path))
                    return default;
            }

            PathTools.CreateAbsFolderPath(path);

            var settings = AssetDatabase.LoadAssetAtPath(path, type);
            if (settings == null)
            {
                var newProfile = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(newProfile, path);
                AssetDatabaseTools.SaveRefresh();

                settings = AssetDatabase.LoadAssetAtPath(path, type);
            }
            return settings;
        }

        public static T CreateGetInstance<T>(string path="") where T : ScriptableObject
        {
            return (T)CreateGetInstance(typeof(T),path);
        }


        /// <summary>
        /// Get scriptable's serializedObject 
        /// if path is empty will find from AssetPathAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="so"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static SerializedObject GetSerializedInstance(Type type,string path = "")
        {
            // check path from attribute
            if (string.IsNullOrEmpty(path))
            {
                path = SOAssetPathAttribute.GetPath(type);

                if (string.IsNullOrEmpty(path))
                    return default;
            }
            // from cache or get new
            return cachedSerializedObjects.Get(path, () => new SerializedObject(CreateGetInstance(type, path)));
        }
        public static SerializedObject GetSerializedInstance<T>(string path="") where T : ScriptableObject
        {
            return GetSerializedInstance(typeof(T), path);
        }


        /// <summary>
        /// Create ScriptableObject, save to {PathTools.PATH_POWER_UTILITIES}/{folderName}/{filePrefixName}_{nextId}.asset
        /// </summary>
        /// <param name="settingSOType"></param>
        /// <param name="subFolderName"></param>
        /// <param name="filePrefixName"></param>
        public static Object CreateSettingSO(Type settingSOType, string subFolderName, string filePrefixName)
        {
            var nextId = Resources.FindObjectsOfTypeAll(settingSOType).Length + 1;
            var path = $"{PathTools.PATH_POWER_UTILITIES}/{subFolderName}/{filePrefixName}_{nextId}.asset";
            var newSO = ScriptableObjectTools.CreateGetInstance(settingSOType, path);
            return newSO;
        }
    }
}
#endif