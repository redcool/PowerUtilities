#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using UnityEngine.Experimental.Rendering;

    [CustomPropertyDrawer(typeof(EnumSearchableAttribute))]
    public class EnumSearchableAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EnumSearchableAttribute;
            if(attr.enumType == null && property.propertyType == SerializedPropertyType.Enum)
            {
                attr.enumType = fieldInfo.FieldType;
            }

            var pos = position;
            pos.width = EditorGUIUtility.labelWidth;

            EditorGUI.LabelField(pos, label);

            pos.x += pos.width;
            pos.width = position.width - pos.width;
            var isClicked = GUI.Button(pos, Enum.GetName(attr.enumType, property.enumValueIndex), EditorStyles.popup);

            if (isClicked)
            {
                var provider = ScriptableObject.CreateInstance<EnumSearchProvider>();
                provider.windowTitle = attr.enumType.Name;
                provider.isReadTextFile = attr.isReadTextFile;
                provider.onSelectedChanged = enumValue =>
                {
                    property.serializedObject.Update();
                    property.enumValueIndex = (int)enumValue;
                    property.serializedObject.ApplyModifiedProperties();
                };
                provider.enumType = attr.enumType;

                var winPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                SearchWindow.Open(new SearchWindowContext(winPos), provider);
            }
        }
    }
}
#endif