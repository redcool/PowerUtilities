namespace PowerUtilities
{
using System;
using System.Collections;
using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomPropertyDrawer(typeof(LayerIndexAttribute))]
    public class LayerIndexDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.LayerField(position, property.displayName, property.intValue);
            EditorGUI.EndProperty();
        }
    }
#endif
    /// <summary>
    /// like GameObject's Layer
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LayerIndexAttribute : PropertyAttribute
    {

    }
}
