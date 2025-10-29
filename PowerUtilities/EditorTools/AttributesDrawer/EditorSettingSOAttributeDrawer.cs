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
        readonly GUIContent GUI_CREATE_BUTTON = new GUIContent();
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
            if (isTargetEditorFolded)
            {
                DrawCreateListItem(property, attr.listPropName,ref subSO);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw Create New{Type} gui,
        /// </summary>
        /// <param name="property"></param>
        /// <param name="listPropName"></param>
        /// <param name="listSO">list serializedObject</param>
        private void DrawCreateListItem(SerializedProperty property, string listPropName,ref SerializedObject listSO)
        {
            if (string.IsNullOrEmpty(listPropName) || !property.objectReferenceValue)
                return;

            if (listSO == null || listSO.targetObject != property.objectReferenceValue)
                listSO = new SerializedObject(property.objectReferenceValue);

            listSO.UpdateIfRequiredOrScript();
            var listProp = listSO.FindProperty(listPropName);

            var listFieldType = property.objectReferenceValue.GetFieldInfoHierarchy(listPropName, out var _);
            var listGenericType = listFieldType.FieldType.GetGenericArguments().FirstOrDefault();

            GUI_CREATE_BUTTON.Set($"Create New {listGenericType.Name}", $"create {listGenericType.Name}, append {listPropName}", null);
            if (GUILayout.Button(GUI_CREATE_BUTTON))
            {
                var newSO = ScriptableObjectTools.CreateSettingSO(listGenericType, property.name, listGenericType.Name);
                listProp.AppendElement().objectReferenceValue = newSO;
                listSO.ApplyModifiedProperties();
            }
        }
    }
}

#endif