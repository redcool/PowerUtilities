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
    /// Draw a ramap slider in (or not in) group .
    /// 
    /// material ui slider range is [0,1]
    /// </summary>
    public class GroupSliderDrawer : BaseGroupItemDrawer
    {

        GroupAPITools.SliderType sliderType;
        public GroupSliderDrawer(string groupName) : this(groupName,"") { }
        public GroupSliderDrawer(string groupName,string tooltip) : base(groupName,tooltip) { }
        /// <summary>
        /// type : remap(default) ,int,float
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="tooltip"></param>
        /// <param name="type"></param>
        public GroupSliderDrawer(string groupName, string tooltip, string type) : base(groupName, tooltip)
        {
            Enum.TryParse(type, out sliderType);
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.GetPropertyType() != (int)UnityEngine.Rendering.ShaderPropertyType.Range)
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
            value = GroupAPITools.DrawSlider(position, label, value, prop.rangeLimits, sliderType);

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = value;
            }
        }
    }
}
#endif