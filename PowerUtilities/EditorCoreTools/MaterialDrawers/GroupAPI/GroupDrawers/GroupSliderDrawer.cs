#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.Linq;
    using System;
    using Codice.Client.BaseCommands;
    using UnityEditor.Experimental.GraphView;

    /// <summary>
    /// Draw a ramap slider in (or not in) group .
    /// 
    /// material ui slider range is [0,1]
    /// </summary>
    public class GroupSliderDrawer : BaseGroupItemDrawer
    {

        GroupAPITools.SliderType sliderType;
        public GroupSliderDrawer(string groupName) : this(groupName,"") { }
        public GroupSliderDrawer(string groupName,string tooltip) : base(groupName,tooltip) { }
        public GroupSliderDrawer(string groupName, string tooltip, string type) : base(groupName, tooltip)
        {
            Enum.TryParse(type, out sliderType);
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type != MaterialProperty.PropType.Range)
            {
                EditorGUI.LabelField(position, $"{label} not range");
                return;
            }

            EditorGUIUtility.labelWidth = MaterialGroupTools.BASE_LABLE_WIDTH;

            DrawSlider(sliderType,position, prop, label);
        }

        void DrawSlider(GroupAPITools.SliderType sliderType, Rect position, MaterialProperty prop, GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            var value = prop.floatValue;

            switch (sliderType)
            {
                case GroupAPITools.SliderType.@float:
                    value = EditorGUI.Slider(position, label, value, prop.rangeLimits.x, prop.rangeLimits.y);
                    break;
                case GroupAPITools.SliderType.@int:
                    value = EditorGUI.IntSlider(position, label, (int)value, (int)prop.rangeLimits.x, (int)prop.rangeLimits.y);
                    break;
                default:
                    value = EditorGUITools.DrawRemapSlider(position, prop.rangeLimits, label, value);
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value;
            }
        }
    }
}
#endif