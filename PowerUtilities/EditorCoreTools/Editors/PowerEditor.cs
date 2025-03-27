#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// helper CustomEditor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class PowerEditor<T> : Editor where T : class
    {
        /// <summary>
        /// Show version in inspector when not empty
        /// </summary>
        public virtual string Version { get; } ="";
        public virtual string TitleHelpStr { get; } = "";

        /// <summary>
        /// Use uiElement must override this
        /// otherwise use IMGUI
        /// </summary>
        /// <returns></returns>
        public virtual bool IsUseUIElements() => false;

        public (string, bool) foldInfo = ("Options",true);
        /// <summary>
        /// current instance
        /// </summary>
        public T inst;

        /// <summary>
        /// Get root element
        /// </summary>
        /// <returns></returns>
        public override VisualElement CreateInspectorGUI()
        {
            inst = target as T;

            if (!IsUseUIElements())
                return null;

            // adjust root container
            var root = new VisualElement();
            root.style.marginTop = 10;

            return root;
        }
        /// <summary>
        /// Load uxml ,clone into container
        /// </summary>
        /// <param name="container"></param>
        /// <param name="uxmlFileName"></param>
        public void LoadUXML(VisualElement container,string uxmlFileName)
        {
            if (string.IsNullOrEmpty(uxmlFileName))
                return;

            // load uxml
            var nameExt = uxmlFileName.SplitFileNameExt();
            var layoutAsset = AssetDatabaseTools.FindAssetPathAndLoad<VisualTreeAsset>(out var _, nameExt.name, nameExt.ext, true);
            layoutAsset.CloneTree(container);
        }

        public override void OnInspectorGUI()
        {
            inst = target as T;

            if (IsUseUIElements())
                return;

            GUILayout.Space(0);

            if (!string.IsNullOrEmpty(TitleHelpStr))
            {
                //EditorGUITools.BeginVerticalBox(() =>
                //{
                EditorGUILayout.HelpBox(TitleHelpStr, MessageType.Info);
                //});
            }

            if (!string.IsNullOrEmpty(Version))
            {
                var lastRect = GUILayoutUtility.GetLastRect();
                var rect = new Rect(80, lastRect.yMax, 100, 18);

                EditorGUITools.DrawColorLabel(rect, new GUIContent(Version), Color.cyan);
            }

            serializedObject.UpdateIfRequiredOrScript();

            if (NeedDrawDefaultUI())
            {
                if (DrawDefaultInspector())
                {
                    OnInspectorGUIChanged(inst);
                }
            }
            DrawInspectorUI(inst);
            EditorGUITools.BeginVerticalBox(() =>
            {
            });

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// draw coding
        /// </summary>
        /// <param name="inst"></param>
        public virtual void DrawInspectorUI(T inst) { }
        /// <summary>
        /// Draw Default Inspector
        /// </summary>
        /// <returns></returns>
        public virtual bool NeedDrawDefaultUI() => false;

        /// <summary>
        /// callback fro EditorGUI.EndChangeCheck() is true
        /// </summary>
        public virtual void OnInspectorGUIChanged(T inst) { }
    }
}
#endif