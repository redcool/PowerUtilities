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
    [CustomPropertyDrawer(typeof(SortingLayerIndexAttribute))]
    public class SoringLayerIndexAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var viewWidth = EditorGUIUtility.currentViewWidth;

            var sortingLayerNames = SortingLayer.layers.Select(item => item.name).ToArray();

            EditorGUI.BeginProperty(position, label, property);
            // label
            position.width = EditorGUIUtility.labelWidth; // 0.618f
            EditorGUI.PrefixLabel(position, label);

            // sorting layers
            position.x += position.width + 2;
            position.width = viewWidth - position.x;
            property.intValue = EditorGUI.Popup(position, property.intValue, sortingLayerNames);

            EditorGUI.EndProperty();
        }
    }
#endif
    /// <summary>
    /// show SortingLlayer in editor gui, add this to int field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SortingLayerIndexAttribute : PropertyAttribute
    {

    }
}
