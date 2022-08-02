#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using System.IO;

namespace PowerUtilities
{
    public class AssetDatabaseTools
    {
        /// <summary>
        /// 找寻assetPaths下面的gameObject并返回T组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="assetPaths"></param>
        /// <returns></returns>
        public static T[] FindComponentFromAssets<T>(string filter, params string[] assetPaths)
            where T : Component
        {
            var gos = FindAssetsInProject<GameObject>(filter + " t:GameObject", assetPaths);
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

        /// <summary>
        /// Create a folder in scene folder, folder's name is scene's name
        /// 
        /// </summary>
        /// <param name="subFolderName"></param>
        /// <returns></returns>
        public static string CreateFolderSameNameAsScene()
        {
            var scene = SceneManager.GetActiveScene();
            var sceneParentFolder = PathTools.GetAssetDir(scene.path);
            var sceneFolder = $"{sceneParentFolder}/{scene.name}";
            //1 create folder same as scene's Name
            if (!AssetDatabase.IsValidFolder(sceneFolder))
            {
                sceneFolder = CreateFolder(sceneParentFolder, scene.name);
                AssetDatabase.Refresh();
            }

            return sceneFolder;
        }

        /// <summary>
        /// Create new folder,return path string
        /// </summary>
        /// <param name="parentFolder"></param>
        /// <param name="subFolder"></param>
        /// <returns>folder assets path</returns>
        public static string CreateFolder(string parentFolder, string subFolder)
        {
            var guid = AssetDatabase.CreateFolder(parentFolder, subFolder);
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        public static string[] GetSelectedFolders()
        {
            return Selection.objects.Select(item =>
            {
                var path = AssetDatabase.GetAssetPath(item);
                var isFolder = AssetDatabase.IsValidFolder(path);
                if (!isFolder)
                    path = Path.GetDirectoryName(path);

                return path;
            }).ToArray();
        }
    }
}
#endif