#if UNITY_EDITOR
namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;
    using UnityEditor;
    using System.IO;
    using System;
    using System.Linq;
    using Object = UnityEngine.Object;
    using System.Collections.Generic;
    using System.Text;
    using PowerUtilities;

    public static class EditorTools
    {
        public static T Save<T>(byte[] bytes, string assetPath)
            where T : Object
        {
            var path = PathTools.GetAssetAbsPath(assetPath);
            File.WriteAllBytes(path, bytes);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static void SaveAsset(Object target)
        {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        #region Selection
        public static T[] GetFilteredFromSelection<T>(SelectionMode mode) where T : Object
        {
            var objs = Selection.GetFiltered(typeof(T), mode);
            return Array.ConvertAll(objs, t => (T)t);
        }

        public static T GetFirstFilteredFromSelection<T>(SelectionMode mode) where T : Object
        {
            var objs = GetFilteredFromSelection<T>(mode);
            if (objs.Length > 0)
                return objs[0];
            return default(T);
        }

        /// <summary>
        /// 获取 选中物体目录的路径
        /// </summary>
        /// <returns></returns>
        public static string[] GetSelectedObjectAssetFolder()
        {
            var gos = Selection.objects;

            var q = gos.Select(go => {
                var path = AssetDatabase.GetAssetPath(go);
                if (string.IsNullOrEmpty(path))
                    return "";
                // selected a folder
                if (Directory.Exists(path))
                    return path;

                return PathTools.GetAssetPath(Path.GetDirectoryName(path));
            });

            return q.Where(p => !string.IsNullOrEmpty(p)).ToArray();
        }

        #endregion

        #region Scene
        public static void ForeachSceneObject<T>(Action<T> act) where T : Object
        {
            if (act == null)
                return;

            var objs = Object.FindObjectsOfType<T>();
            foreach (var item in objs)
            {
                act(item);
            }
        }


        #endregion

        #region StaticEditorFlags

        public static bool HasStaticFlag(this GameObject go, StaticEditorFlags flag)
        {
            var flags = GameObjectUtility.GetStaticEditorFlags(go);
            return (flags & flag) == flag;
        }

        public static void RemoveStaticFlags(this GameObject go, StaticEditorFlags flags)
        {
            if (!go)
                return;

            var existFlags = GameObjectUtility.GetStaticEditorFlags(go);
            GameObjectUtility.SetStaticEditorFlags(go, existFlags & ~flags);
        }

        public static void AddStaticFlags(this GameObject go, StaticEditorFlags flags)
        {
            if (!go)
                return;

            var existFlags = GameObjectUtility.GetStaticEditorFlags(go);
            GameObjectUtility.SetStaticEditorFlags(go, existFlags | flags);
        }
        #endregion
        
    }
}
#endif