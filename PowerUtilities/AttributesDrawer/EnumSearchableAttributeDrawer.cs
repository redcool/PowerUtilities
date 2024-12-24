#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;
    using static UnityEditor.PlayerSettings;

    [CustomPropertyDrawer(typeof(EnumSearchableAttribute))]
    public class EnumSearchableAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EnumSearchableAttribute;
            // check enumType
            if (attr.enumType == null && property.propertyType == SerializedPropertyType.Enum)
            {
                attr.enumType = fieldInfo.FieldType;
            }

            var pos = position;
            pos.width = EditorGUIUtility.labelWidth;
            // 1 label
            EditorGUI.LabelField(pos, label);

            //2 button(popup)
            pos.x += pos.width;
            pos.width = position.width - pos.width;
            // property.intValue save enumValue rather than enumValueIndex
            var isClicked = GUI.Button(pos, Enum.GetName(attr.enumType, property.intValue), EditorStyles.popup);
            if (isClicked)
                ShowSearchWindow(property, attr);

        }
        static void ShowSearchWindow(SerializedProperty property, EnumSearchableAttribute attr)
        {
            var provider = SearchWindowTools.CreateProvider<EnumSearchProvider>();
            provider.windowTitle = attr.enumType.Name;
            provider.textFileName = attr.textFileName;
            provider.onSelectedChanged = enumValue =>
            {
                property.serializedObject.Update();
                //property.enumValueIndex = enumValueIndex;  //dont use this value, cause bug
                property.intValue = enumValue; // property.intValue save enumValue rather than enumValueIndex
                property.serializedObject.ApplyModifiedProperties();
            };
            provider.enumType = attr.enumType;

            SearchWindowTools.OpenSearchWindow(provider);
        }
    }
}
#endif