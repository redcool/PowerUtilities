#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Material's ToggleOff in groupDrawer API
    /// group dont exist , will not indent
    /// 
    /// [GroupToggleOff(ShadowGroup,_RECEIVE_SHADOW_OFF)]_ReceiveShadow("_ReceiveShadow",int) = 1
    /// </summary>
    public class GroupToggleOffDrawer : BaseGroupItemDrawer
    {
        string keyword;
        public GroupToggleOffDrawer() : this("", "") { }
        public GroupToggleOffDrawer(string groupName):this(groupName,""){}
        public GroupToggleOffDrawer(string groupName,string keyword) : this(groupName, keyword, "") { }
        public GroupToggleOffDrawer(string groupName, string keyword,string tooltip) : base(groupName,tooltip)
        {
            this.keyword = keyword;
        }

        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            bool isOn = Mathf.Abs(prop.floatValue) > 0.001f;
            isOn = EditorGUI.Toggle(position, label, isOn);

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = isOn ? 1 : 0;

                if (!string.IsNullOrEmpty(keyword))
                {
                    MaterialPropertyTools.SetKeyword(prop, keyword, !isOn);
                }
            }
        }
    }
}
#endif