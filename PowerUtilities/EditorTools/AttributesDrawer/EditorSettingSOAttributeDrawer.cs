#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    [CustomPropertyDrawer(typeof(EditorSettingSOAttribute))]
    public class EditorSettingSOAttributeDrawer : PropertyDrawer
    {
        Editor targetEditor;
        bool isTargetEditorFolded = true;
        Type settingSOType;

        SerializedObject subSO; // property is (MonoBehavriour, ScriptableObject)
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 0;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorSettingSOAttribute;
			settingSOType = attr.settingType??fieldInfo.FieldType;

            var serializedObject = property.serializedObject;

            serializedObject.UpdateIfRequiredOrScript();
            // draw property
            EditorGUITools.DrawSettingSO(property, ref targetEditor, ref isTargetEditorFolded, settingSOType);

            // draw create list item
            if (isTargetEditorFolded && !string.IsNullOrEmpty(attr.listPropName) && property.objectReferenceValue)
            {
                //var itemType = fieldInfo.FieldType.GetGenericArguments().FirstOrDefault();
                //Debug.Log(itemType);
                if (subSO == null)
                    subSO = new SerializedObject(property.objectReferenceValue);

                subSO.UpdateIfRequiredOrScript();
                var listProp = subSO.FindProperty(attr.listPropName);

                var listFieldType = property.objectReferenceValue.GetFieldInfoHierarchy(attr.listPropName, out var _);
                var listGenericType = listFieldType.FieldType.GetGenericArguments().FirstOrDefault();

                if (GUILayout.Button($"Create New {listGenericType.Name}"))
                {
                    var newSO = ScriptableObjectTools.CreateSettingSO(listGenericType, property.name, listGenericType.Name);
                    listProp.AppendElement().objectReferenceValue = newSO;
                    subSO.ApplyModifiedProperties();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

    }
}

#endif