#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Material's vector property ui
    /// 
    /// </summary>
    public class GroupVectorSliderDrawer : BaseGroupItemDrawer
    {
        const char ITEM_SPLITTER = ' ';
        readonly string[] strings_XYZ = new string[] { "X","Y","Z"};

        string[] headers;
        Vector2[] ranges;

        /// <summary>
        /// null : default, remap float,
        /// float : float slider
        /// int : int slider
        /// </summary>
        string sliderStyleFormat;

        //public GroupVectorSliderDrawer(string headerString) : this("",headerString, "") { }
        public GroupVectorSliderDrawer(string headerString,string rangeString) : this("", headerString, rangeString) { }
        /// <summary>
        /// headerString 
        ///     4slider : a b c d, [space] is splitter
        ///     vector3 slider1 : VectorSlider(vname sname ,0_1)
        /// rangeString like m1_1 0_1 ,[space][_] is splitter, m1: -1
        /// 
        /// tooltip : helps
        /// 
        /// Demos:
        /// 
        /// sliders 4 : 
        /// *(headers length == ranges length)
        /// [GroupVectorSlider(group1, a b c d, 0_1 1_2 0_1 0_m2)] _Vector("_Vector", vector) = (1,1,1,1)
        /// 
        /// vector3 slider 1 :
        /// * (headers length ==2 && ranges length == 1)
        /// [GroupVectorSlider(group1,Dir(xyz) intensity, 0_1)] _Vector("_Vector2", vector) = (1,0.1,0,1)
        /// 
        /// 2 int sliders:
        /// [GroupVectorSlider(SheetAnimation,RowCount ColumnCount,1_16 1_16,,int)]_MainTexSheet("_MainTexSheet",vector)=(1,1,1,1)
        /// </summary>
        /// <param name="headerString"></param>
        public GroupVectorSliderDrawer(string groupName, string headerString, string rangeString) : this(groupName, headerString,rangeString, "","") { }
        public GroupVectorSliderDrawer(string groupName, string headerString, string rangeString,string sliderFormat) : this(groupName, headerString, rangeString, "", sliderFormat) { }
        public GroupVectorSliderDrawer(string groupName,string headerString,string rangeString,string tooltip,string sliderStyleFormat) : base(groupName,tooltip)
        {
            if (!string.IsNullOrEmpty(headerString))
            {
                headers = headerString.Split(ITEM_SPLITTER);
            }
            InitRanges(rangeString);
            this.sliderStyleFormat = sliderStyleFormat;
        }

        private void InitRanges(string rangeString)
        {
            if (string.IsNullOrEmpty(rangeString))
                return;
            
            ranges = GroupAPITools.TranslateRangeStr(rangeString);
        }
        

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (MaterialGroupTools.IsGroupOn(groupName))
                return (headers.Length + 1) * LINE_HEIGHT;

            return MIN_LINE_HEIGHT;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (prop.type != MaterialProperty.PropType.Vector || headers == null)
            {
                editor.DrawDefaultInspector();
                return;
            }
            // restore width
            EditorGUIUtility.labelWidth = MaterialGroupTools.BASE_LABLE_WIDTH;

            EditorGUI.BeginChangeCheck();
            var value = prop.vectorValue;

            // property label
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width, LINE_HEIGHT), label);

            EditorGUI.indentLevel++;

            position.y += LINE_HEIGHT;
            position.height -= LINE_HEIGHT;

            if (IsVector3Slider1()) // draw vector and float
                DrawVector3Slider1(position, ref value);
            else // draw 4 float
                DrawSliders(position, ref value);

            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                prop.vectorValue = value;
            }
        }

        private bool IsVector3Slider1()
        {
            return headers.Length == 2 && ranges.Length == 1;
        }

        private void DrawVector3Slider1(Rect position, ref Vector4 value)
        {
            var vectorHeader = headers[0];
            var sliderHeader = headers[1];


            var itemWidth = position.width / 4;
            var pos = position;
            pos.height = LINE_HEIGHT;
            pos.width = itemWidth;

            EditorGUI.LabelField(pos, vectorHeader);

            EditorGUIUtility.labelWidth = 30;// EditorStyles.label.CalcSize(new GUIContent("X")).x;
            for (int i = 0; i < 3; i++)
            {
                pos.x += itemWidth;

                value[i] = EditorGUI.FloatField(pos, strings_XYZ[i], value[i]);
            }
            // slider
            pos.x = position.x ;
            pos.y += LINE_HEIGHT;
            pos.width = position.width;
            EditorGUIUtility.labelWidth = MaterialGroupTools.BASE_LABLE_WIDTH;
            value[3] = EditorGUITools.DrawRemapSlider(pos, ranges[0], new GUIContent(sliderHeader), value[3]);
        }

        private void DrawSliders(Rect position, ref Vector4 value)
        {
            var pos = new Rect(position.x, position.y, position.width, 18);
            for (int i = 0; i < headers.Length; i++)
            {
                DrawSlider(ref value, pos, i);

                pos.y += LINE_HEIGHT;

            }
        }

        private void DrawSlider(ref Vector4 value, Rect pos, int i)
        {
            if (string.IsNullOrEmpty(sliderStyleFormat))
            {
                value[i] = EditorGUITools.DrawRemapSlider(pos, ranges[i], new GUIContent(headers[i]), value[i]);
                return;
            }

            if (sliderStyleFormat == "int")
            {
                value[i] = (int)value[i];
            }

            value[i] = EditorGUI.Slider(pos, new GUIContent(headers[i]), value[i], ranges[i].x, ranges[i].y);
        }
    }
}
#endif