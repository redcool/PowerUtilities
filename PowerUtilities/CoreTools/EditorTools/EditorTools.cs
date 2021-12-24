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
#if UNITY_5
            var objs = Selection.GetFiltered(typeof(Object),mode);
            var list = new List<T>(objs.Length);
            foreach (var obj in objs)
            {
                var t = obj as T;
                if (t)
                    list.Add(t);
            }
            return list.ToArray();
#else
            var objs = Selection.GetFiltered(typeof(T), mode);
            return Array.ConvertAll(objs, t => (T)t);
#endif
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
        #region ScriptableObject
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
        #region AssetDatabase Tools
        /// <summary>
        /// 找寻assetPaths下面的gameObject并返回T组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="assetPaths"></param>
        /// <returns></returns>
        public static T[] FindComponentFromAssets<T>(string filter,params string[] assetPaths)
            where T : Component
        {
            var gos = FindAssetsInProject<GameObject>(filter+" t:GameObject", assetPaths);
            var q = gos.Select(go => go.GetComponent<T>());
            return q.ToArray();
        }


        /// <summary>
        /// 找寻assetPaths下面的T Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="searchInFolders"></param>
        /// <returns></returns>
        public static T[] FindAssetsInProject<T>(string filter, params string[] searchInFolders)
            where T : Object
        {
            var paths = AssetDatabase.FindAssets(filter, searchInFolders);
            var q = paths.Select(pathStr => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(pathStr)));
            return q.ToArray();
        }

        public enum SearchFilter
        {
            GameObject, Texture, Material, Shader
        }

        public static T[] FindAssetsInProject<T>(SearchFilter filterEnum, params string[] searchInFolders)
            where T : Object
        {
            var filter = "t:" + Enum.GetName(typeof(SearchFilter), filterEnum);
            return FindAssetsInProject<T>(filter, searchInFolders);
        }

        public static T[] FindAssetsInProjectByType<T>(params string[] folders) where T : UnityEngine.Object
        {
            var filter = "t:" + typeof(T).Name;
            var paths = AssetDatabase.FindAssets(filter, folders);
            var q = paths.Select(pathStr => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(pathStr)));
            return q.ToArray();
        }
        #endregion
    }
}
#endif