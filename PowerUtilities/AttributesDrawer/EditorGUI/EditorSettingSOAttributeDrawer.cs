#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    [CustomPropertyDrawer(typeof(EditorSettingSOAttribute))]
    public class EditorSettingSOAttributeDrawer : PropertyDrawer
    {
        Editor targetEditor;
        bool isTargetEditorFolded = true;
        Type settingSOType;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorSettingSOAttribute;
            property.TryGetType(ref settingSOType);

            var serializedObject = property.serializedObject;

            EditorGUIUtility.labelWidth = 250;
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUITools.DrawSettingSO(property, ref targetEditor, ref isTargetEditorFolded, settingSOType);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif