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
        public GroupSpaceDecorator(string groupName,float height):base(groupName,"")
        {
            spaceHeight = height;
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return MaterialGroupTools.IsGroupOn(groupName) ? LINE_HEIGHT : MIN_LINE_HEIGHT;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!MaterialGroupTools.IsGroupOn(groupName))
                return;

            EditorGUI.DrawRect(position,new Color(0,0,0,0));
        }
    }
}
