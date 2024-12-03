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
            // update tooltip
            var attr = attribute as SortingLayerIndexAttribute;
            label.tooltip = attr.tooltip;

            // gui
            var viewWidth = EditorGUIUtility.currentViewWidth;
            var sortingLayerNames = SortingLayer.layers.Select(item => new GUIContent(item.name)).ToArray();
            property.intValue = EditorGUI.Popup(position, label, property.intValue, sortingLayerNames);

        }
    }
#endif
    /// <summary>
    /// show SortingLlayer in editor gui, add this to int field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SortingLayerIndexAttribute : PropertyAttribute
    {
        public string tooltip;
    }
}
