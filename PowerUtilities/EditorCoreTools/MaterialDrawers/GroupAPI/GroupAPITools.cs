#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class GroupAPITools
    {

        public enum SliderType
        {
            remap // float value,ui show [0,1] slider
            , @float // float value, ui show float slider
            , @int  // int value,ui show int slider
            ,field  // float value,ui show float value
        }

        /// <summary>
        /// Translate "a12.34_10 0_1.23 0_1 0_1" to Vector2[]
        /// </summary>
        /// <param name="rangeString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector2[] TranslateRangeStr(string rangeString)
        {
            if (string.IsNullOrEmpty(rangeString))
                throw new ArgumentNullException(nameof(rangeString));

            // a12.34_10 0_1.23 0_1 m0.2_1
            const string pattern = @"([a-zA-Z]?)(\.?\d+\.?\d*)";
            var ms = Regex.Matches(rangeString, pattern);
            
            var count = ms.Count/2;
            var items = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                var a = StrToFloat(ms[i*2]);
                var b = StrToFloat(ms[i*2+1]);
                items[i].Set(a, b);
            }
            return items;
        }
        static float StrToFloat(Match m)
        {
            float num = Convert.ToSingle(m.Groups[2].Value);

            // negative
            if (!string.IsNullOrEmpty(m.Groups[1].Value))
                num *= -1;
            return num;
        }

        public static float DrawSlider(Rect pos, string header,float value, Vector2 range,SliderType sliderType)
        => DrawSlider(pos,EditorGUITools.TempContent(header),value,range,sliderType);
        

        public static float DrawSlider(Rect pos, GUIContent header, float value, Vector2 range, SliderType sliderType)
        {
            switch (sliderType)
            {
                case SliderType.@int:
                case SliderType.@float:
                    if (sliderType == SliderType.@int)
                        value = Mathf.RoundToInt(value);

                    return EditorGUI.Slider(pos, header, value, range.x, range.y);
                case SliderType.field:
                    return EditorGUI.FloatField(pos, header, value);
                default: // remap
                    return EditorGUITools.DrawRemapSlider(pos, range, header, value);
            }
        }
    }
}
#endif