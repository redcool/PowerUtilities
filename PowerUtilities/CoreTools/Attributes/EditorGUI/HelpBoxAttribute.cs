namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
    public class HelpBoxAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as HelpBoxAttribute;
            var lineCount = Mathf.Max(2,attr.lineCount);
            return base.GetPropertyHeight(property, label) * lineCount;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.HelpBox(position, property.stringValue, MessageType.Info);
            EditorGUI.EndProperty();
        }
    }
#endif
    /// <summary>
    /// Show string as HelpBox
    /// </summary>
    public class HelpBoxAttribute : PropertyAttribute
    {
        public int lineCount = 2;
    }
}
