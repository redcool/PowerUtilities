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
        /// Attempts to retrieve a <see cref="Type"/> from the editor assembly or the application domain based on the
        /// specified type name.
        /// </summary>
        /// <remarks>This method first attempts to resolve the type from the assembly containing the <see
        /// cref="Editor"/> class. If the type is not found, it searches the application domain for a type with the
        /// specified name.</remarks>
        /// <param name="type">A reference to a <see cref="Type"/> variable. If the variable is <see langword="null"/>, it will be assigned
        /// the resolved type if found.</param>
        /// <param name="typeName">The fully qualified name of the type to retrieve. This parameter cannot be <see langword="null"/> or empty.</param>
        public static void GetTypeFromEditorAssembly(ref Type type, string typeName)
        {
            if (type == null)
                type = Assembly.GetAssembly(typeof(Editor)).GetType(typeName);
            if (type == null)
                type = ReflectionTools.GetAppDomainTypes<Type>(type => type.FullName == typeName).FirstOrDefault();

            Debug.Assert(type != null);
        }

        /// <summary>
        /// Displays a dialog box with "OK" and "Cancel" options and a warning message.
        /// </summary>
        /// <param name="text">The message to display in the dialog box.</param>
        /// <returns><see langword="true"/> if the "OK" button is clicked; otherwise, <see langword="false"/> if the "Cancel"
        /// button is clicked.</returns>
        public static bool DisplayDialog_Ok_Cancel(string text)
            => EditorUtility.DisplayDialog("Warning", text, "ok", "cancel");

        /// <summary>
        /// Displays a progress bar indicating the progress of an operation.
        /// </summary>
        /// <remarks>The progress bar is updated based on the ratio of <paramref name="id"/> to <paramref
        /// name="count"/>. When <paramref name="id"/> equals <paramref name="count"/>, the progress bar is
        /// cleared.</remarks>
        /// <param name="id">The current progress step, where 0 represents the start of the operation.</param>
        /// <param name="count">The total number of steps in the operation. Must be greater than 0.</param>
        public static void DisplayProgress(int id, int count)
        {
            EditorUtility.DisplayProgressBar("Progress", "Export Progress", (float)id / count);
            if (id == count)
                EditorUtility.ClearProgressBar();
        }
    }
}
#endif