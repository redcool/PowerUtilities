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
        /// <summary>
        /// Get or create instance 
        /// if path is empty will find from AssetPathAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="so"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Object GetInstance(Type type,string path = "")
        {
            // check path from attribute
            if (string.IsNullOrEmpty(path))
            {
                path = SOAssetPathAttribute.GetPath(type);
            }

            PathTools.CreateAbsFolderPath(path);

            var settings = AssetDatabase.LoadAssetAtPath(path, type);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        public static T GetInstance<T>(string path="") where T : ScriptableObject
        {
            return (T)GetInstance(typeof(T),path);
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
            }

            return new SerializedObject(GetInstance(type,path));
        }
        public static SerializedObject GetSerializedInstance<T>(string path="") where T : ScriptableObject
        {
            return GetSerializedInstance(typeof(T), path);
        }

    }
}
#endif