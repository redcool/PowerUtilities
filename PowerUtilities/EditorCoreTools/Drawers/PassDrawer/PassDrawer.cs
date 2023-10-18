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
            //reset global width
            EditorGUIUtility.fieldWidth = 200;

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
            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, label, true);
            var pos = GUILayoutUtility.GetLastRect();
            pos.width = 200;

            // show checker on pass title
            pos.x += pos.width;
            pos.width = 20;

            var enabledProp = passItemSO.FindProperty("enabled");
            if (enabledProp != null)
            {
                enabledProp.boolValue = EditorGUI.ToggleLeft(pos, "", enabledProp.boolValue);

                if (Event.current.IsMouseDown() && pos.Contains(Event.current.mousePosition))
                {
                    enabledProp.boolValue = true;
                }
            }

            // show pass cameraTag
            pos.x += pos.width;
            pos.width = 200;

            var gameCameraTag = passItemSO.FindProperty("gameCameraTag");
            if (gameCameraTag != null)
            {
                var cameraTag = string.IsNullOrEmpty(gameCameraTag.stringValue) ? "All Camera" : gameCameraTag.stringValue;
                EditorGUI.LabelField(pos, $"{cameraTag} run");
            }
        }
    }
}
#endif