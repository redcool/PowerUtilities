#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Numerics;
    using UnityEditor;
    using UnityEngine;
    using Vector4 = UnityEngine.Vector4;

    /// <summary>
    /// Draw MinMaxSlider
    ///     Vector4(min,max,minLimit,maxLimit)
    /// demo:
    ///     [GroupMinMaxSlider(Group)]
    ///     _VectorValue0("_VectorValue0", vector) = (0,0.1,0,1)
    /// </summary>
    public class GroupMinMaxSliderDrawer : BaseGroupItemDrawer
    {
        public GroupMinMaxSliderDrawer() : this("", "") { }
        public GroupMinMaxSliderDrawer(string groupName):this(groupName,""){}
        public GroupMinMaxSliderDrawer(string groupName, string tooltip) : base(groupName, tooltip)
        { 
            
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            var v = prop.vectorValue;
            CorrectMinMax(ref v);

            var rowWidth = EditorGUIUtility.currentViewWidth - (MaterialGroupTools.IsGroupOn(groupName) ? 30 : 0);
            var rowHeight = EditorGUIUtility.singleLineHeight;
            var floatSize = 80;
            var labelWidth = 100;

            var labelPos = new Rect(position.x,position.y, labelWidth, rowHeight);
            var minValuePos = new Rect(labelPos.xMax,position.y, floatSize, rowHeight);

            var sliderWidth = Mathf.Max(1, rowWidth-labelWidth-floatSize*2);
            var sliderPos = new Rect(minValuePos.xMax,position.y, sliderWidth, rowHeight);
            var maxValuePos = new Rect(sliderPos.xMax,position.y, floatSize, rowHeight);

            EditorGUI.BeginChangeCheck();

            EditorGUIUtility.labelWidth = 40;
            EditorGUI.LabelField(labelPos, label);
            v.x = Mathf.Clamp(EditorGUI.FloatField(minValuePos, "Min", v.x), v.z, v.w);
            EditorGUI.MinMaxSlider(sliderPos, ref v.x, ref v.y, v.z, v.w);
            v.y = Mathf.Clamp(EditorGUI.FloatField(maxValuePos, "Max", v.y), v.z, v.w);

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = v;
            }
        }

        private void CorrectMinMax(ref Vector4 v)
        {
            if (v.z >= v.w)
                v.w +=0.1f;

        }
    }
}
#endif