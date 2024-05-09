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

            if(!string.IsNullOrEmpty(TitleHelpStr))
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.HelpBox(TitleHelpStr, MessageType.Info);
                EditorGUILayout.EndVertical();
            }

            if (!string.IsNullOrEmpty(Version))
            {
                if (!string.IsNullOrEmpty(Version))
                {
                    var rect = new Rect(100, 4, 100, 18);
                    if (!string.IsNullOrEmpty(TitleHelpStr))
                    {
                        var lastRect = GUILayoutUtility.GetLastRect();
                        rect.y += lastRect.max.y;
                    }

                    EditorGUITools.DrawColorLabel(rect, new GUIContent(Version), Color.cyan);
                }

            }


            if (NeedDrawDefaultUI())
            {
                EditorGUI.BeginChangeCheck();
                DrawDefaultInspector();
                if(EditorGUI.EndChangeCheck())
                {
                    OnInspectorGUIChanged(inst);
                }

            }
            
            serializedObject.UpdateIfRequiredOrScript();

            GUILayout.BeginVertical("Box");
            DrawInspectorUI(inst);
            GUILayout.EndVertical();

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