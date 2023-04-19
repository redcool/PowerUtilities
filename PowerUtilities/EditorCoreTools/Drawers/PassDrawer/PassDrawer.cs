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

            EditorGUITools.DrawColorUI(() =>
            {
                foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, label, true);
            }, GUI.contentColor, labelColor);

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