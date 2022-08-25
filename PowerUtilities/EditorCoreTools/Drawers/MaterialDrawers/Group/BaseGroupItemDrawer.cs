#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using System;

    /// <summary>
    /// Group Item ui
    /// </summary>
    public abstract class BaseGroupItemDrawer : MaterialPropertyDrawer
    {
        string groupName;
        string tooltip;

        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }

        public BaseGroupItemDrawer(string groupName,string tooltip)
        {
            this.groupName = groupName;
            this.tooltip = tooltip;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (MaterialGroupTools.IsGroupOn(groupName))
            {
                var baseHeight = MaterialEditor.GetDefaultPropertyHeight(prop);
                return baseHeight;
            }
            
            return -2;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!MaterialGroupTools.IsGroupOn(groupName))
                return;

            label.tooltip = tooltip;
            var lastLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUI.indentLevel += MaterialGroupTools.GroupIndentLevel(GroupName);
            DrawGroupUI(position,prop,label,editor);
            EditorGUI.indentLevel -= MaterialGroupTools.GroupIndentLevel(groupName);
            EditorGUIUtility.labelWidth = lastLabelWidth;
        }

        public abstract void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor);
    }
}
#endif