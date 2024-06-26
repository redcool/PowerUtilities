#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;

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

        public (string, bool) foldInfo = ("Options",true);



        public override void OnInspectorGUI()
        {
            var inst = target as T;

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
                var rect = new Rect(80, lastRect.yMax + 3, 100, 18);

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