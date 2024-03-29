﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Material's Toggle in group
    /// : group dont exist , will not indent
    /// 
    /// [GroupToggle(ShadowGroup,_Ker)]_ReceiveShadow("_ReceiveShadow",int) = 1
    /// </summary>
    public class GroupToggleDrawer : BaseGroupItemDrawer
    {
        string keyword;
        public GroupToggleDrawer() : this("", "") { }
        public GroupToggleDrawer(string groupName):this(groupName,""){}
        public GroupToggleDrawer(string groupName,string keyword) : this(groupName, keyword, "") { }
        public GroupToggleDrawer(string groupName, string keyword,string tooltip) : base(groupName,tooltip)
        {
            this.keyword = keyword;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            // sync checkbutton with keyword
            prop.SyncKeywordToFloat(editor,keyword);

            // user op
            EditorGUI.BeginChangeCheck();
            bool isOn = Mathf.Abs(prop.floatValue) > 0.001f;
            isOn = EditorGUI.Toggle(position, label, isOn);

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = isOn ? 1 : 0;

                if (!string.IsNullOrEmpty(keyword))
                {
                    MaterialPropertyTools.SetKeyword(prop, keyword, isOn);
                }
            }
        }


    }
}
#endif