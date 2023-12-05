namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
    public class EnumFlagsAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EnumFlagsAttribute;
            if (attr.isFlags)
            {
                property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);
            }
            else
            {
                property.intValue = EditorGUI.Popup(position, label.text, property.intValue, property.enumNames);
            }
        }
    }

#endif

    /// <summary>
    /// flags Enum
    /// like LayerMask
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class EnumFlagsAttribute : PropertyAttribute
    {
        public bool isFlags;
        public EnumFlagsAttribute(bool IsFlags = true)
        {
            this.isFlags = IsFlags;
        }
    }
}