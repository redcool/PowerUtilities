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


            var pos = position;
            pos.width = EditorGUIUtility.labelWidth;

            EditorGUI.LabelField(pos, label);
            pos.x += pos.width;
            pos.width = position.width - pos.width;
            var isClicked = GUI.Button(pos, Enum.GetName(attr.enumType, property.enumValueIndex), EditorStyles.popup);

            if (isClicked)
            {
                var provider = new EnumSearchProvider()
                {
                    windowTitle = attr.enumType.Name,
                    isReadTextFile = attr.isReadTextFile,
                    onSelectedChanged = enumValue =>
                    {
                        property.serializedObject.Update();
                        property.enumValueIndex = (int)enumValue;
                        property.serializedObject.ApplyModifiedProperties();
                    },
                    enumType = attr.enumType,
                };

                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
            }
        }
    }
}
#endif