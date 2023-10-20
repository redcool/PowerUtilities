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
            EditorGUIUtility.fieldWidth = 50;

            passItemSO.UpdateIfRequiredOrScript();

            // pass header
            EditorGUITools.DrawColorUI(() =>
            {
                DrawPassTitleRow(passItemSO, foldoutProp, label);

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

        private static void DrawPassTitleRow(SerializedObject passItemSO, SerializedProperty foldoutProp, GUIContent label)
        {
            var e = Event.current;
            var pos = EditorGUILayout.GetControlRect();

            pos.width = 200;
            //EditorGUI.DrawRect(pos, Color.red);
            foldoutProp.boolValue = EditorGUI.Foldout(pos, foldoutProp.boolValue, label,true);

            pos.x += pos.width;
            pos.width = 40;

            var enabledProp = passItemSO.FindProperty("enabled");
            if(enabledProp != null)
            {
                enabledProp.boolValue = EditorGUI.Toggle(pos, enabledProp.boolValue);
            }

            pos.x += pos.width;
            pos.width = 200;
            var gameCameraTag = passItemSO.FindProperty("gameCameraTag");
            if(gameCameraTag != null)
            {
                var cameraTag = string.IsNullOrEmpty(gameCameraTag.stringValue) ? "All Camera" : gameCameraTag.stringValue;
                EditorGUI.LabelField(pos, $"({cameraTag}) run");
            }
        }
    }
}
#endif