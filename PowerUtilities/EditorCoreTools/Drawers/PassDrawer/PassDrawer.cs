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
        static readonly GUIContent guiSceneView = new GUIContent("S", "pass can run in SceneView"),
            guiEditorOnly = new GUIContent("E", "only run in Unity Editor"),
            enabledGUI = new GUIContent("","enable this pass"),
            gameCameraTagGUI = new GUIContent("","which camera (tag) can run this pass");

        public static void DrawPassDetail(SerializedObject passItemSO, Color labelColor, SerializedProperty foldoutProp, GUIContent label)
        {
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
                EditorGUITools.DrawDefaultInspect(passItemSO,out _);
                GUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            passItemSO.ApplyModifiedProperties();
        }

        public static void DrawPassDetail(Editor passItemEditor, Color labelColor, SerializedProperty foldoutProp, GUIContent label)
        {
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
                EditorGUI.LabelField(pos, GUIContentEx.TempContent($"T: {cameraTag}", gameCameraTagGUI.tooltip));
            }

            //----- scene only
            ShowSpecialFlags(passItemSO, ref pos, "cameraType", guiSceneView);
            //----- editor only
            ShowSpecialFlags(passItemSO, ref pos, "isEditorOnly", guiEditorOnly);
        }

        private static void ShowSpecialFlags(SerializedObject passItemSO, ref Rect pos,string featureFieldName,GUIContent flagGUIContent)
        {
            var prop = passItemSO.FindProperty(featureFieldName);
            if (prop == null)
                return;

            var isValid = prop.propertyType switch
            {
                SerializedPropertyType.Boolean => prop.boolValue,
                SerializedPropertyType.Enum => prop.intValue > 1,
                _ => false
            };
            
            if (isValid)
            {
                pos.x += pos.width+3;
                pos.width = 15;
                GUI.Label(pos, flagGUIContent, "Box");
            }
        }
    }
}
#endif