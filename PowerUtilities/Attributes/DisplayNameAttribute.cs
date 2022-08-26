using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(DisplayNameAttribute))]
    public class DisplayNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            var attr = attribute as DisplayNameAttribute;
            var ranges = fieldInfo.GetCustomAttributes(typeof(RangeAttribute), true);

            label.text = attr.name;
            label.tooltip = attr.tooltip;

            if (ranges.Length > 0)
            {
                var range = ranges[0] as RangeAttribute;
                EditorGUI.Slider(position, property, range.min, range.max, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
#endif

    /// <summary>
    /// Replace property name in editor gui
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class DisplayNameAttribute : PropertyAttribute
    {
        public string name;
        public string tooltip;

        public DisplayNameAttribute(string name, string tooltip=null)
        {
            this.name = name;
            this.tooltip=tooltip;
        }

        public override string ToString()
        {
            return name;
        }
    }

}
