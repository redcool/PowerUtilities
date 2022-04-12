#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace PowerUtilities {
    /// <summary>
    /// Material's Enum Attribute
    /// GroupEnum(groupName,keywords,isKeyword)
    /// 
    /// [GroupEnum(ShadowGroup,A 0 B 1)]_Keys("_Keys",int) = 0
    /// [GroupEnum(ShadowGroup,A 0 B 1,true)]_Keys("_Keys",int) = 0
    /// </summary>
    public class GroupEnumDrawer : MaterialPropertyDrawer
    {
        public const char KEY_VALUE_SPLITTER = ' '; // space char
        string groupName;
        bool isKeyword;
        Dictionary<string, int> keywordValueDict = new Dictionary<string, int>();
        public GroupEnumDrawer() : this(MaterialGroupTools.DEFAULT_GROUP_NAME, "","") { }
        public GroupEnumDrawer(string groupName,string enumName):this(groupName,enumName,""){}
        public GroupEnumDrawer(string groupName, string keywordString,string keyword)
        {
            this.groupName = groupName;
            isKeyword = !string.IsNullOrEmpty(keyword);

            if (!string.IsNullOrEmpty(keywordString))
            {
                if (keywordString.Contains(KEY_VALUE_SPLITTER))
                    ParseKeyValuePairs(keywordString);
                else
                    ParseEnum(keywordString);

            }
        }

        private void ParseEnum(string keywordString)
        {
             
        }

        private void ParseKeyValuePairs(string keywordString)
        {
            var items = keywordString.Split(KEY_VALUE_SPLITTER);
            var len = items.Length;
            if (!isKeyword)
                len = len / 2;

            for (int i = 0; i < len; i++)
            {
                if (!isKeyword)
                    keywordValueDict[items[i * 2]] = Convert.ToInt32(items[i * 2 + 1]); // put [k,v]
                else
                    keywordValueDict[items[i]] = 0; // put k
            }
        }

        public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
        {
            if (MaterialGroupTools.IsGroupOn(groupName))
                return base.GetPropertyHeight(prop, label, editor);
            return 0;
        }

        public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            if (!MaterialGroupTools.IsGroupOn(groupName))
                return;

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            var index = (int)prop.floatValue;

            var keys = keywordValueDict.Keys.ToArray();
            index = EditorGUI.Popup(position,label.text, index, keys);

            EditorGUI.indentLevel--;
            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = index;

                if (isKeyword)
                {
                    for (int i = 0; i < keys.Length; i++)
                    {
                        MaterialPropertyDrawerTools.SetKeyword(prop, keys[i], i == index);
                    }
                }
            }
        }
    }
}
#endif