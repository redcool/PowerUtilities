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
    [CustomPropertyDrawer(typeof(EditorNotNull))]
    public class EditorNotNullDrawer : PropertyDrawer
    {
        public bool IsPropertyTypeValid(SerializedProperty property)
        {
            var isValid = true;
            if (property.propertyType == SerializedPropertyType.ObjectReference || property.propertyType == SerializedPropertyType.Generic)
                isValid = property.objectReferenceValue != null;
            else if (property.propertyType == SerializedPropertyType.String)
                isValid = !string.IsNullOrEmpty(property.stringValue);

            return isValid;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var isValid = IsPropertyTypeValid(property);

            GUI.contentColor = !isValid ? Color.red : Color.white;

            EditorGUI.PropertyField(position, property, label,true);
        }
    }
#endif
    /// <summary>
    /// Field can't be null in editor, show red if it is null
    /// </summary>
    public class EditorNotNull : PropertyAttribute
    {

    }
}
