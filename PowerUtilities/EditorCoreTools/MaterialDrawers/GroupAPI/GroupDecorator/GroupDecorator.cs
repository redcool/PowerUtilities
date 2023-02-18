#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Draw a group 
    /// Decorator can add multiple to material property
    /// 
    /// demos:
    /// [Group(group0,group0 helps,true,_KEY1 _KEY2)] _FloatVlaue0("_FloatVlaue0",range(0,1)) = 0.1
    /// </summary>
    public class GroupDecorator : MaterialPropertyDrawer
    {
        string groupName;
        string tooltip;
        bool hasCheckedMark;

        bool isUpdateProp;
        string[] keywords;

        public GroupDecorator(string groupName) : this(groupName, "") { }
        public GroupDecorator(string groupName, string tooltip):this(groupName, tooltip, null) { }
        public GroupDecorator(string groupName, string tooltip,string checkedMark) : this(groupName, tooltip, checkedMark, null) { }
        public GroupDecorator(string groupName, string tooltip, string updateProp, string keywordsWhenChecked)
        {
            this.tooltip = tooltip;
            this.groupName = groupName;
            this.isUpdateProp = !string.IsNullOrEmpty(updateProp) && updateProp != "false";
            hasCheckedMark = isUpdateProp ||! string.IsNullOrEmpty(keywordsWhenChecked);

            if (!string.IsNullOrEmpty(keywordsWhenChecked))
                keywords = keywordsWhenChecked.Split(' ');

            var groupInfo = EditorPrefTools.Get<MaterialGroupTools.GroupInfo>(groupName);
            if (groupInfo != null)
                MaterialGroupTools.SetState(groupName, groupInfo.isOn, groupInfo.hasCheckedMark, groupInfo.isChecked);
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();

            var groupInfo = MaterialGroupTools.GetGroupInfo(groupName);
            groupInfo.hasCheckedMark = hasCheckedMark;

            DrawHeader(position,prop,editor, groupInfo);

            if (EditorGUI.EndChangeCheck())
            {
                // write to register
                EditorPrefTools.Set(groupName, groupInfo);
            }
        }

        private void DrawHeader(Rect position, MaterialProperty prop, MaterialEditor editor, MaterialGroupTools.GroupInfo groupInfo)
        {
            if (hasCheckedMark)
            {
                EditorGUITools.ToggleFoldoutHeader(position, EditorGUITools.TempContent(groupName, tooltip),
                    ref groupInfo.isOn, ref groupInfo.isChecked);

                if (keywords != null && keywords.Length != 0)
                {
                    //ShaderEx.SetKeywords(keywords, groupInfo.isChecked);
                    var mat = (Material)editor.target;
                    if(mat)
                        mat.SetKeywords(keywords,groupInfo.isChecked);
                }

                if (isUpdateProp)
                    prop.floatValue = groupInfo.isChecked ? 1 : 0;
            }
            else
            {
                groupInfo.isOn = EditorGUI.BeginFoldoutHeaderGroup(position, groupInfo.isOn,
                    EditorGUITools.TempContent(groupName, tooltip));

                EditorGUI.EndFoldoutHeaderGroup();
            }
        }
    }
}
#endif