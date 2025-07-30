using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PowerUtilities
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(TooltipsAttribute))]
    public class TooltipsAttributeDrawer : DecoratorDrawer
    {
        
        public override void OnGUI(Rect position)
        {
            var attr = (TooltipsAttribute)attribute;
            var style = EditorStyles.label;
            style.fontStyle = FontStyle.BoldAndItalic;
            //style.contentOffset = new Vector2(10, 0);
            EditorGUI.SelectableLabel(position, attr.tooltip, EditorStyles.whiteMiniLabel);
        }
    }
#endif

    /// <summary>
    /// Show tooltips in decorator line
    /// </summary>
    public class TooltipsAttribute : PropertyAttribute
    {
        public string tooltip = "";
        public TooltipsAttribute(string tips)
        {
            tooltip = tips;
        }
    }
}