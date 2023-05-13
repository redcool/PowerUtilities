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
        public static T[] FindComponentInProject<T>(string filter, params string[] searchInFolders)
            where T : Component
        {
            var gos = FindAssetsInProject<GameObject>(filter, searchInFolders);
            var q = gos.Select(go => go.GetComponent<T>());
            return q.ToArray();
        }


        /// <summary>
        /// Find asset in searchInFolders, return T[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filter"></param>
        /// <param name="searchInFolders"></param>
        /// <returns></returns>
        public static T[] FindAssetsInProject<T>(string filter="", params string[] searchInFolders)
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
        /// 
        /// </summary>
        /// <param name="subFolderName"></param>
        /// <returns></returns>
        public static string CreateSceneFolder(Scene scene=default)
        {
            if(scene == default)
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
        public static string CreateFolder(string parentFolder, string subFolder)
        {
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
        public static string[] FindAssetPaths(string filter,string extName=null,params string[] searchInFolders)
        {
            var q = AssetDatabase.FindAssets(filter, searchInFolders)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => extName == null ? true : Path.GetExtension(path) == extName)
                ;
            return q.ToArray();
        }
    }
}
#endif