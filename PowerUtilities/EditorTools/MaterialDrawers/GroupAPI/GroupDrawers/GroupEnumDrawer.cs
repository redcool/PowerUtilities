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
    /// : group dont exist , will not indent
    /// 
    /// [GroupEnum(ShadowGroup,A 0 B 1)]_Keys("_Keys",int) = 0
    /// [GroupEnum(ShadowGroup,key1 key2,true)]_Keys("_Keys",int) = 0
    /// </summary>
    public class GroupEnumDrawer : BaseGroupItemDrawer
    {
        public const char KEY_VALUE_SPLITTER = ' '; // space char
        bool isKeyword;
        Dictionary<string, int> keywordValueDict = new Dictionary<string, int>();
        //public GroupEnumDrawer() : this("", "","") { }
        //public GroupEnumDrawer(string enumName) : this("", enumName) { }
        public GroupEnumDrawer(string groupName, string enumName) : this(groupName, enumName, "","") { }
        public GroupEnumDrawer(string groupName, string enumName, string keyword) : this(groupName, enumName, keyword, "") { }
        public GroupEnumDrawer(string groupName, string enumName,string keyword,string tooltip) : base(groupName,tooltip)
        {
            isKeyword = !string.IsNullOrEmpty(keyword);

            if (!string.IsNullOrEmpty(enumName))
            {
                if (enumName.Contains(KEY_VALUE_SPLITTER))
                    ParseKeyValuePairs(enumName);
                else
                    ParseEnum(enumName);

            }
        }

        private void ParseEnum(string enumName)
        {
            var dlls = AppDomain.CurrentDomain.GetAssemblies();
            string[] names = null;

            //foreach (var d in dlls)
            //{
            //    if (d.FullName.StartsWith("UnityEngine,"))
            //    {
            //        var t = d.GetType(enumName);
            //        names = Enum.GetNames(t);
            //        break;
            //    }
            //}

            var enumType = TypeCache.GetTypesDerivedFrom(typeof(Enum)).Where(t => t.FullName == enumName).FirstOrDefault();
            if (enumType == null)
                return;

            names = Enum.GetNames(enumType);

            foreach (var name in names)
            {
                keywordValueDict.Add(name, (int)Enum.Parse(enumType,name));
            }
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
                    keywordValueDict[items[i]] = i; // put k
            }
        }


        public override void DrawGroupUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
        {
            EditorGUI.BeginChangeCheck();
            var matPropValue = (int)prop.floatValue;
            var index = keywordValueDict.Values.FindIndex(val => val == matPropValue);

            var displayOptions = keywordValueDict.Keys.Select(item=>new GUIContent(item)).ToArray();
            index = EditorGUI.Popup(position,label, index, displayOptions);

            if (EditorGUI.EndChangeCheck())
            {
                prop.floatValue = keywordValueDict[displayOptions[index].text];

                if (isKeyword)
                {
                    for (int i = 0; i < displayOptions.Length; i++)
                    {
                        MaterialPropertyTools.SetKeyword(prop, displayOptions[i].text, i == index);
                    }
                }
            }
        }

    }
}
#endif