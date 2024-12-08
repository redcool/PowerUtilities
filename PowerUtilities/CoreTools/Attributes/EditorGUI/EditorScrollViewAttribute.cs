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

    [CustomPropertyDrawer(typeof(EditorScrollViewAttribute))]
    public class EditorScrollViewAttributeDrawer : PropertyDrawer
    {
        Vector2 scrollPos;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorScrollViewAttribute;
            var lineCount = Mathf.Max(2,attr.lineCount);
            return base.GetPropertyHeight(property, label) * lineCount;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            scrollPos = GUI.BeginScrollView(position, scrollPos, position);
            EditorGUI.PropertyField(position, property, label);
            GUI.EndScrollView();
        }
    }
#endif
    /// <summary>
    /// Show string as HelpBox
    /// </summary>
    public class EditorScrollViewAttribute : PropertyAttribute
    {
        public int lineCount = 2;
    }
}
