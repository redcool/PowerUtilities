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
        /// Find gameObjects in searchInFolders, return gameObject's component
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="searchInFolders"></param>
        /// <returns></returns>
        public static T[] FindComponentsInProject<T>(string filter, params string[] searchInFolders)
            where T : Component
        {
            return FindAssetsInProject<GameObject>(filter, searchInFolders)
                .Where(go => go.GetComponent<T>())
                .Select(go => go.GetComponent<T>())
                .ToArray();
        }


        /// <summary>
        /// Find asset in searchInFolders, return T[]
        /// 
        /// * Global search is slow, results should be cached.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="searchInFolders"></param>
        /// <returns></returns>
        public static T[] FindAssetsInProject<T>(string filter = "", params string[] searchInFolders)
            where T : Object
        {
            var paths = AssetDatabase.FindAssets($"{filter} t:{typeof(T).Name}", searchInFolders);

            var q = paths.Select(pathStr => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(pathStr)))
                .Where(item => item);
            return q.ToArray();
        }

        //public enum SearchFilter
        //{
        //    Object, GameObject, Texture, Material, Shader, AnimationClip, AudioClip
        //}

        //public static T[] FindAssetsInProject<T>(SearchFilter filterEnum, params string[] searchInFolders)
        //    where T : Object
        //{
        //    var filter = "t:" + Enum.GetName(typeof(SearchFilter), filterEnum);
        //    return FindAssetsInProject<T>(filter, searchInFolders);
        //}

        //public static T[] FindAssetsInProjectByType<T>(params string[] folders) where T : UnityEngine.Object
        //{
        //    var filter = "t:" + typeof(T).Name;
        //    var paths = AssetDatabase.FindAssets(filter, folders);
        //    var q = paths.Select(pathStr => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(pathStr)));
        //    return q.ToArray();
        //}

        /// <summary>
        /// Create scene adjoint folder
        /// if not exists create it
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static string CreateGetSceneFolder(Scene scene = default)
        {
            if (scene == default)
                scene = SceneManager.GetActiveScene();

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
        public static string CreateFolder(string parentFolder, string subFolder, bool isUseExist = false)
        {
            if (isUseExist)
            {
                var splitter = parentFolder.EndsWith("/") ? "" : "/";

                var path = parentFolder + splitter + subFolder;
                PathTools.CreateAbsFolderPath(path);
                return path;
            }
            // create new folder when exist
            var guid = AssetDatabase.CreateFolder(parentFolder, subFolder);
            return AssetDatabase.GUIDToAssetPath(guid);
        }

        /// <summary>
        /// Find assets in project
        /// 
        /// extName is null dont check extName, use empty string check that files(folders) have no extName
        /// </summary>
        /// <param name="filter">filter string</param>
        /// <param name="extName">null dont check extName, empty string check that files has no extName</param>
        /// <param name="searchInFolders"></param>
        /// <returns></returns>
        public static string[] FindAssetsPath(string filter, string extName = null, bool isWholeWordsMatch = false, params string[] searchInFolders)
        {
            var q = AssetDatabase.FindAssets(filter, searchInFolders)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                    (extName == null ? true : Path.GetExtension(path).EndsWith(extName))
                    && (isWholeWordsMatch ? filter == Path.GetFileNameWithoutExtension(path) : true)
                )
                ;
            var results = q.ToArray();
            return results;
        }
        /// <summary>
        /// Find assets in project and load them
        /// 
        /// extName is null dont check extName, use empty string check that files(folders) have no extName
        /// </summary>
        /// <param name="filter">filter string</param>
        /// <param name="extName">null dont check extName, empty string check that files has no extName</param>
        /// <param name="searchInFolders"></param>
        /// <returns></returns>
        public static T[] FindAssetsPathAndLoad<T>(out string[] paths, string filter, string extName = null, bool isWholeWordsMatch = false, params string[] searchInFolders)
            where T : Object
        {
            paths = FindAssetsPath(filter, extName, isWholeWordsMatch, searchInFolders);
            return paths.Select(path => AssetDatabase.LoadAssetAtPath<T>(path)).ToArray();
        }

        public static string FindAssetPath(string filter, string extName = null, bool isWholeWordsMatch = false, params string[] searchInFolders)
            => FindAssetsPath(filter, extName, isWholeWordsMatch, searchInFolders).FirstOrDefault();

        public static T FindAssetPathAndLoad<T>(out string path, string filter, string extName = null, bool isWholeWordsMatch = false, params string[] searchInFolders)
            where T : Object
        {
            if (!string.IsNullOrEmpty(extName) && extName.StartsWith("."))
                extName = extName.Substring(1);

            path = FindAssetPath(filter, extName, isWholeWordsMatch, searchInFolders);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void SaveRefresh()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void DeleteAsset(string assetPath, bool refresh)
        {
            if (string.IsNullOrEmpty(assetPath))
                return;

            AssetDatabase.DeleteAsset(assetPath);
            PathTools.CreateAbsFolderPath(assetPath);

            if (refresh)
            {
                SaveRefresh();
            }
        }

        /// <summary>
        /// get a asset from scene's folder
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pathInSceneFolder"></param>
        /// <returns></returns>
        public static T LoadAssetAtSceneFolder<T>(string pathInSceneFolder) where T : Object
        {
            if (string.IsNullOrEmpty(pathInSceneFolder))
                return default;

            var sceneFolder = CreateGetSceneFolder();
            var path = $"{sceneFolder}/{pathInSceneFolder}";
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// create asset in scene's folder
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="pathInSceneFolder"></param>
        public static void CreateAssetAtSceneFolder(Object asset, string pathInSceneFolder)
        {
            if (string.IsNullOrEmpty(pathInSceneFolder))
                return;

            var sceneFolder = CreateGetSceneFolder();
            var path = $"{sceneFolder}/{pathInSceneFolder}";
            AssetDatabase.CreateAsset(asset, path);
        }

        public static T CreateAssetThenLoad<T>(Object asset, string pathInAssets, bool deleteExistAsset = false) where T : Object
        {
            if (string.IsNullOrEmpty(pathInAssets))
                return default;

            if (deleteExistAsset)
                AssetDatabase.DeleteAsset(pathInAssets);
            // get exists asset
            var assetExists = AssetDatabase.LoadAssetAtPath<T>(pathInAssets);
            if (assetExists)
            {
                return assetExists;
            }

            // create new asset
            AssetDatabase.CreateAsset(asset, pathInAssets);
            return AssetDatabase.LoadAssetAtPath<T>(pathInAssets);
        }

        public static bool IsAssetExist(string pathInAssets)
        {
            if (string.IsNullOrEmpty(pathInAssets))
                return default;

            var absPath = PathTools.GetAssetAbsPath(pathInAssets);
            return File.Exists(absPath);
        }

        public static void AddObjectToAsset(Object objectToAdd, Object assetObject, bool isClearSubAsset = false)
        {
            if (isClearSubAsset)
            {
                RemoveSubAssets(assetObject);
            }

            AssetDatabase.AddObjectToAsset(objectToAdd, assetObject);
        }

        public static void RemoveSubAssets(Object assetObject)
        {
            var path = AssetDatabase.GetAssetPath(assetObject);
            var items = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
            foreach (var item in items)
            {
                AssetDatabase.RemoveObjectFromAsset(item);
            }
        }

        /// <summary>
        /// Get Asset folder path
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetAssetFolder(Object obj)
        {
            if (!obj)
                return "";

            var path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path))
                return "";

            var isFolder = AssetDatabase.IsValidFolder(path);
            if (!isFolder)
                path = Path.GetDirectoryName(path);

            return path;
        }
        /// <summary>
        /// Add BundleName, bundleName is null, generate a GUID
        /// </summary>
        /// <param name="bundleName"></param>
        public static string AddAssetBundleName(string bundleName = null)
        {
            if (string.IsNullOrEmpty(bundleName))
                bundleName = GUID.Generate().ToString();

            var rootImp = AssetImporter.GetAtPath("Assets");
            rootImp.assetBundleName = bundleName;
            rootImp.SaveAndReimport();

            rootImp.assetBundleName = "";
            rootImp.SaveAndReimport();

            return bundleName;
        }

        public static int RenameAssetBundleName(string oldName, string newName)
        {

            var paths = FindAssetsPath($"b:{oldName}");
            foreach (var path in paths)
            {
                var imp = AssetImporter.GetAtPath(path);
                imp.assetBundleName = newName;
                imp.SaveAndReimport();
            }

            // abName unused
            if (paths.Length == 0)
            {
                AddAssetBundleName(newName);
            }

            // force remove oldName
            AssetDatabase.RemoveAssetBundleName(oldName, true);

            return paths.Length;
        }

        /// <summary>
        /// [
        ///     1 [ ...]
        ///     2 [....]
        /// ]
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="dependencyList"></param>
        public static void FindAssetBundleDependenciesRecursive(string abName, ref List<List<string>> dependencyList)
        {
            var dependencies = AssetDatabase.GetAssetBundleDependencies(abName, false);
            if (dependencies.Length == 0)
                return;

            var list = new List<string>(dependencies);
            dependencyList.Add(list);

            foreach (var item in list)
            {
                FindAssetBundleDependenciesRecursive(item, ref dependencyList);
            }

        }
    }
}
#endif