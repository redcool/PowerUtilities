#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// GroupAPI's Space like material attribute [Space(10)] 
    /// 
    /// testcase :
    /// [GroupSpace(FoamAndCaustics,10)] _Float("Float",float) = 0
    /// </summary>
    public class GroupSpaceDecorator : BaseGroupItemDrawer
    {
        public float spaceHeight=1;
        public Color backgroundColor;

        public GroupSpaceDecorator(string groupName, float height):this(groupName, height, null) { }
        public GroupSpaceDecorator(string groupName, float height, string backgroundColorStr = "#ffffff00") : base(groupName, "")
        {
            spaceHeight = height;

            if (!string.IsNullOrEmpty(backgroundColorStr))
                ColorUtility.TryParseHtmlString(backgroundColorStr, out backgroundColor);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return MaterialGroupTools.IsGroupOn(groupName) ? spaceHeight : MIN_LINE_HEIGHT;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!MaterialGroupTools.IsGroupOn(groupName) || backgroundColor == default)
                return;

            EditorGUI.DrawRect(position,backgroundColor);
        }
    }
}
#endif