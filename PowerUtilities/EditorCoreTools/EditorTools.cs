﻿#if UNITY_EDITOR
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
    using System.Reflection;

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
        
        /// <summary>
        /// CreateEditor with cached
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="targetObject"></param>
        /// <param name="editor"></param>
        public static void CreateEditor<T>(T targetObject,ref Editor editor) where T : Object
        {
            if (!targetObject)
                return;

            if (editor == null || targetObject != editor.target)
            {
                editor = Editor.CreateEditor(targetObject);
            }
        }

        public static void CreateEditor<T>(T[] targetObject, ref Editor editor) where T : Object
        {
            if (editor == null || !ArrayUtility.ArrayEquals(targetObject, editor.targets))
            {
                editor = Editor.CreateEditor(targetObject);
            }
        }

        /// <summary>
        /// Get or create Editor from UnityEditor.dll
        /// like:
        /// EditorTools. GetUnityEditor(ref spriteRendererEditor,target, "SpriteRendererEditor");
        /// </summary>
        /// <param name="editorInst">current editor instance</param>
        /// <param name="target">editor wrapper target</param>
        /// <param name="editorType">editor type in UnityEditor.dll</param>
        /// <param name="editorTypeName">class name</param>
        public static void GetOrCreateUnityEditor(ref Editor editorInst, Object[] targets,ref Type editorType, string editorTypeName)
        {
            GetTypeFromEditorAssembly(ref editorType,editorTypeName);

            if (editorInst == null)
                editorInst = Editor.CreateEditor(targets, editorType);
        }

        /// <summary>
        /// Get Type from UnityEditor.dll
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeName"></param>
        public static void GetTypeFromEditorAssembly(ref Type type,string typeName)
        {
            //editorType = typeof(Editor).Assembly.GetTypes()
            //    .Where(t => t.Name == editorTypeName)
            //    .FirstOrDefault()
            //    ;

            if(type == null)
                type = Assembly.GetAssembly(typeof(Editor)).GetType(typeName);
        }

        public static bool DisplayDialog_Ok_Cancel(string text)
            => EditorUtility.DisplayDialog("Warning", text, "ok", "cancel");

    }
}
#endif