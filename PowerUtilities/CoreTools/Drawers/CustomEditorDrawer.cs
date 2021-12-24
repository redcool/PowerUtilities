namespace PowerUtilities
{
#if UNITY_EDITOR
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;

    /// <summary>
    /// helper CustomEditor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomEditorDrawer<T> : Editor where T : class
    {
        public bool showDefaultUI;

        public override void OnInspectorGUI()
        {
            if (showDefaultUI)
                base.OnInspectorGUI();

            var inst = target as T;
            serializedObject.UpdateIfRequiredOrScript();
            DrawInspectorUI(inst);

            serializedObject.ApplyModifiedProperties();
        }

        public virtual void DrawInspectorUI(T inst)
        {

        }
    }
#endif
}