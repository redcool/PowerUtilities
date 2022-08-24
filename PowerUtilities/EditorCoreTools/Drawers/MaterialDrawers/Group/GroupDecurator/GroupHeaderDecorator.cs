#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{

    public class GroupHeaderDecorator : BaseGroupItemDrawer
    {
        GUIContent headerContent;
        public GroupHeaderDecorator(string header) : this("", header) { }
        public GroupHeaderDecorator(string groupName, string header) : this(groupName, header, "") { }
        public GroupHeaderDecorator(string groupName, string header, string tooltip) : base(groupName, tooltip)
        {
            var text = $"--------{header}--------";
            headerContent = new GUIContent(text, tooltip);
        }
        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return MaterialGroupTools.IsGroupOn(GroupName) ? 18 : -1;
        }
        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            position = EditorGUI.IndentedRect(position);
            EditorGUI.DropShadowLabel(position, headerContent, EditorStyles.boldLabel);
        }
    }
}
#endif