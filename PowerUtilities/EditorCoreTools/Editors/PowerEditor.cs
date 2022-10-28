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

        public void DrawDefaultGUI()
        {
            base.OnInspectorGUI();
        }

        public override void OnInspectorGUI()
        {
            var inst = target as T;
            serializedObject.UpdateIfRequiredOrScript();
            DrawInspectorUI(inst);

            serializedObject.ApplyModifiedProperties();
        }

        public abstract void DrawInspectorUI(T inst);
    }
}
#endif