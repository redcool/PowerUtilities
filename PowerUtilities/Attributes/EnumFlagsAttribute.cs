namespace PowerUtilities
{
#if UNITY_EDITOR

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
        }
    }

#endif

    public class EnumFlagsAttribute : PropertyAttribute
    {
    }
}