#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Show a header
    /// suffix Decorator can overlap multiple, XXXDraw only last one
    /// </summary>
    public class GroupHeaderDecorator : BaseGroupItemDrawer
    {
        GUIContent headerContent;
        public GroupHeaderDecorator(string header) : this("", header) { }
        public GroupHeaderDecorator(string groupName, string header) : this(groupName, header, "") { }
        public GroupHeaderDecorator(string groupName, string header, string tooltip) : base(groupName, tooltip)
        {
            var text = $"-------- <size=14>{header}</size> --------";
            headerContent = new GUIContent(text,tooltip);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return MaterialGroupTools.IsGroupOn(groupName) ? LINE_HEIGHT : 0;
        }
        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            position = EditorGUI.IndentedRect(position);

            var style = EditorStyles.boldLabel;
            style.richText = true;

            EditorGUI.DropShadowLabel(position, headerContent, style);
        }
    }
}
#endif