#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// draw pass details(srp pass, crp pass,...)
    /// </summary>
    public static class PassDrawer
    {
        public static void DrawPassDetail(SerializedObject passItemSO, Color labelColor, SerializedProperty foldoutProp, GUIContent label)
        {
            passItemSO.UpdateIfRequiredOrScript();

            // pass header
            EditorGUITools.DrawColorUI(() =>
            {
                EditorGUILayout.BeginHorizontal();
                // show fold label 
                foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, label, true);

                // show checker
                var enabledProp = passItemSO.FindProperty("enabled");
                if (enabledProp != null)
                {
                    enabledProp.boolValue = EditorGUILayout.Toggle(enabledProp.boolValue);
                }

                EditorGUILayout.EndHorizontal();
            }, GUI.contentColor, labelColor);

            // pass details
            if (foldoutProp.boolValue)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical("Box");
                EditorGUITools.DrawDefaultInspect(passItemSO);
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            passItemSO.ApplyModifiedProperties();
        }
    }
}
#endif