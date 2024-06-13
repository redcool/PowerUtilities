namespace PowerUtilities
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(EditorDisableGroupAttribute))]
    public class EditorDisableGroupEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorDisableGroupAttribute;
            var isDisable = string.IsNullOrEmpty(attr.targetPropName);
            if(!isDisable)
            {
                var prop = property.serializedObject.FindProperty(attr.targetPropName);
                isDisable = prop.intValue == 0;
            }

            EditorGUI.BeginDisabledGroup(isDisable);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif

    /// <summary>
    /// show editor gui in disable mode
    /// </summary>
    public class EditorDisableGroupAttribute : PropertyAttribute
    {
        public string targetPropName;
    }
}
