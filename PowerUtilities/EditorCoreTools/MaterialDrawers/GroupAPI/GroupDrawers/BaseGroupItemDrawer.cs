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
        public static float MIN_LINE_HEIGHT = -2;
        public static float LINE_HEIGHT = EditorGUIUtility.singleLineHeight;

        string tooltip;

        public string groupName;
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
            
            return MIN_LINE_HEIGHT;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!MaterialGroupTools.IsGroupOn(groupName))
                return;

            var isDisabled = MaterialGroupTools.IsGroupDisabled(groupName);
            EditorGUI.BeginDisabledGroup(isDisabled);
            {
                if (!string.IsNullOrEmpty(tooltip))
                    label.tooltip = tooltip;

                var lastLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUI.indentLevel += MaterialGroupTools.GroupIndentLevel(groupName);
                EditorGUI.DrawRect(position, Color.green*0.07f);
                DrawGroupUI(position, prop, label, editor);
                EditorGUI.indentLevel -= MaterialGroupTools.GroupIndentLevel(groupName);
                EditorGUIUtility.labelWidth = lastLabelWidth;
            }
            EditorGUI.EndDisabledGroup();
        }

        public abstract void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor);
    }
}
#endif