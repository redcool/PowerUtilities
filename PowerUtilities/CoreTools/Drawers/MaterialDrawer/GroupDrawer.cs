#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using System.Linq;

    /// <summary>
    /// like this:
    /// [Group(group1,true)]_On("On",float) = 0
    /// [Group(group1)] _Value0("value0", float) = 1
    /// [Group(group1)]_Value1("value1", float) = 1

    /// [Group(group2, true)]_On1("On1", float) = 0
    /// [Group(group2)]_Value2("value2", float) = 1
    /// [Group(group2)]_Value3("value3", float) = 1
    /// </summary>
    public class Group : MaterialPropertyDrawer
    {
        static Dictionary<string, bool> groupDict = new Dictionary<string, bool>();
        string groupName;
        string keyword;
        bool isGroupLabel;

        public static Dictionary<string, bool> GroupDict => groupDict;
        public Group() : this("DefaultGroup", "",""){}
        public Group(string groupName) : this(groupName, "",""){}
        public Group(string groupName, string groupLabel) :this(groupName,groupLabel,""){ }
        public Group(string groupName, string groupLabel,string keyword)
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
                return base.GetPropertyHeight(prop, label, editor);
            if (GroupDict[groupName])
            {
                var baseHeight = base.GetPropertyHeight(prop, label, editor);
                if(prop.type == MaterialProperty.PropType.Texture)
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
            if (!GroupDict.ContainsKey(groupName))
                return;

            if (GroupDict[groupName])
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
                var mats = prop.targets.Select(t => (Material)t);
                foreach (var mat in mats)
                {
                    if (isKeywordOn)
                        mat.EnableKeyword(keyword);
                    else
                        mat.DisableKeyword(keyword);
                }
            }
        }
    }

}
#endif