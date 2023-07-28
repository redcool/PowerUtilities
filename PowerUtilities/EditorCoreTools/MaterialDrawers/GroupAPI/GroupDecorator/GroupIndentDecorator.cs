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
    /// material gui
    /// add indent level
    /// </summary>
    public class GroupIndentDecorator : BaseGroupItemDrawer
    {
        public int indentLevelOffset; 
        public GroupIndentDecorator():this("","","1"){ }
        public GroupIndentDecorator(string indentLevelOffsetStr) : this("", "", indentLevelOffsetStr) { }
        public GroupIndentDecorator(string groupName, string indentLevelOffsetStr) : this(groupName, "", indentLevelOffsetStr) { }
        public GroupIndentDecorator(string groupName, string tooltip, string indentLevelOffsetStr) : base(groupName, tooltip)
        {
            if(!int.TryParse(indentLevelOffsetStr, out indentLevelOffset))
            {
                if (indentLevelOffsetStr.StartsWith('m'))
                    indentLevelOffset = -1 * int.Parse(indentLevelOffsetStr.Substring(1));

            }

        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.indentLevel += indentLevelOffset;
        }
    }
}
#endif