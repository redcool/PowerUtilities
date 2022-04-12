#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    /// <summary>
    /// Material's Group Attribute
    /// 
    /// 
    /// like this:
    /// [Group(group1,true)]_On("On",float) = 0
    /// [Group(group1)] _Value0("value0", float) = 1
    /// [Group(group1)]_Value1("value1", float) = 1

    /// [Group(group2, true)]_On1("On1", float) = 0
    /// [Group(group2)]_Value2("value2", float) = 1
    /// [Group(group2)]_Value3("value3", float) = 1
    /// </summary>
    public class GroupDrawer : MaterialPropertyDrawer
    {
        public const string DEFAULT_GROUP_NAME = "DefaultGroup";

        static Dictionary<string, bool> groupDict = new Dictionary<string, bool>();
        string groupName;
        string keyword;
        bool isGroupLabel;

        public GroupDrawer() : this(DEFAULT_GROUP_NAME, "",""){}
        public GroupDrawer(string groupName) : this(groupName, "",""){}
        public GroupDrawer(string groupName, string groupLabel) :this(groupName,groupLabel,""){ }
        public GroupDrawer(string groupName, string groupLabel,string keyword)
        {
            this.groupName = groupName;
            this.keyword = keyword;
            this.isGroupLabel = !string.IsNullOrEmpty(groupLabel) && groupLabel == "true";

            if (isGroupLabel)
                groupDict[groupName] = EditorPrefs.GetBool(groupName, false);

        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (isGroupLabel)
                return EditorGUIUtility.singleLineHeight;

            if (IsGroupOn(groupName))
            {
                var baseHeight = EditorGUIUtility.singleLineHeight;
                if (prop.type == MaterialProperty.PropType.Texture)
                {
                    baseHeight *= 4;
                }
                return baseHeight;
            }

            return 0;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (isGroupLabel)
            {
                DrawLabel(position, label,prop);
            }
            else
            {
                EditorGUI.indentLevel++;
                DrawContent(position, prop, label, editor);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawContent(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!groupDict.ContainsKey(groupName))
                return;

            if (groupDict[groupName])
            {
                editor.DefaultShaderProperty(position, prop, label.text);
            }
        }
         
        private void DrawLabel(Rect position, GUIContent label, MaterialProperty prop)
        {
            EditorGUI.BeginChangeCheck();
            groupDict[groupName] = EditorGUI.BeginFoldoutHeaderGroup(position, groupDict[groupName], label);
            EditorGUI.EndFoldoutHeaderGroup();

            if (EditorGUI.EndChangeCheck())
            {
                // write to register
                EditorPrefs.SetBool(groupName, groupDict[groupName]);

                // keyword
                var isKeywordOn = groupDict[groupName];
                MaterialPropertyDrawerTools.SetKeyword(prop, keyword,isKeywordOn);
            }
        }

        public static bool IsGroupOn(string groupName)
        {
            if(string.IsNullOrEmpty(groupName) || !groupDict.ContainsKey(groupName))
                return false;
            return groupDict[groupName];
        }

    }



}
#endif