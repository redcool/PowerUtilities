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
        public GroupIndentDecorator():this("","",0){ }
        public GroupIndentDecorator(float indentLevelOffset) : this("", "", indentLevelOffset) { }
        public GroupIndentDecorator(string groupName,float indentLevelOffset) : this(groupName, "", indentLevelOffset) { }
        public GroupIndentDecorator(string groupName, string tooltip, float indentLevelOffset) : base(groupName, tooltip)
        {
            this.indentLevelOffset= (int)indentLevelOffset;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.indentLevel = indentLevelOffset;
        }
    }
}
#endif