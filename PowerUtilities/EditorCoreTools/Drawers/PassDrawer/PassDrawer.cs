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
            EditorGUIUtility.labelWidth = 200;

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

        public static void DrawPassDetail(Editor passItemEditor, Color labelColor, SerializedProperty foldoutProp, GUIContent label)
        {
            EditorGUIUtility.fieldWidth = 50;

            passItemEditor.serializedObject.UpdateIfRequiredOrScript();

            // pass header
            EditorGUITools.DrawColorUI(() =>
            {
                DrawPassTitleRow(passItemEditor.serializedObject, foldoutProp, label);

            }, GUI.contentColor, labelColor);

            // pass details
            if (foldoutProp.boolValue)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginVertical("Box");

                //EditorGUITools.DrawDefaultInspect(passItemEditor);
                passItemEditor.OnInspectorGUI();

                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            passItemEditor.serializedObject.ApplyModifiedProperties();
        }
        private static void DrawPassTitleRow(SerializedObject passItemSO, SerializedProperty foldoutProp, GUIContent label)
        {
            var e = Event.current;
            var pos = EditorGUILayout.GetControlRect();

            //------ foldout
            pos.width = 200;
            pos.height = 20;
            //EditorGUI.DrawRect(pos, Color.red);
            foldoutProp.boolValue = EditorGUI.Foldout(pos, foldoutProp.boolValue, label, true);

            //----- enabled
            pos.x += pos.width;
            pos.width = 45;
            var enabledProp = passItemSO.FindProperty("enabled");
            if (enabledProp != null)
            {
                enabledProp.boolValue = EditorGUI.Toggle(pos, enabledProp.boolValue);
            }
            //-----------gameCameraTag
            pos.x += pos.width;
            pos.width = 150;
            var gameCameraTag = passItemSO.FindProperty("gameCameraTag");
            if (gameCameraTag != null)
            {
                var cameraTag = string.IsNullOrEmpty(gameCameraTag.stringValue) ? "All Camera" : gameCameraTag.stringValue;
                EditorGUI.LabelField(pos, $"T: {cameraTag}");
            }

            //----- scene only
            ShowSpecialFlags(passItemSO, ref pos, "isSceneCameraOnly", "S");
            //----- editor only
            ShowSpecialFlags(passItemSO, ref pos, "isEditorOnly", "E");
        }

        private static void ShowSpecialFlags(SerializedObject passItemSO, ref Rect pos,string featureFieldName,string flagName)
        {
            var boolFieldName = passItemSO.FindProperty(featureFieldName);
            if (boolFieldName != null && boolFieldName.boolValue)
            {
                pos.x += pos.width+3;
                pos.width = 15;
                GUI.Label(pos, flagName,"Box");
            }
        }
    }
}
#endif