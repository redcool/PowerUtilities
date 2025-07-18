#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// add feature: EditorDisableGroup 
    /// </summary>
    [CustomPropertyDrawer(typeof(StringListSearchableAttribute))]
    public class StringListSearchableAttributeDrawer : PropertyDrawer
    {
        bool isIndentAdd1;
        bool isGroupOn;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // check EditorGroup attr
            CheckGroupAttr(ref isIndentAdd1,ref isGroupOn);

            return isGroupOn? base.GetPropertyHeight(property, label) : -2;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!isGroupOn)
                return;

            var attr = attribute as StringListSearchableAttribute;
            // check enumType
            if (attr.enumType == null && property.propertyType == SerializedPropertyType.Enum)
            {
                attr.enumType = fieldInfo.FieldType;
            }

            var pos = position;
            pos.width = position.width - 20;

            // check EditorDisableGroupAttribute
            CheckEditorDisableGroupAttr(out var isDisabledisDisable);
            EditorGUI.BeginDisabledGroup(isDisabledisDisable);
            //EditorGUITools.DrawIndent(() =>
            //{
                EditorGUI.PropertyField(pos, property, label);
            //}, isIndentAdd1, 1);
            EditorGUI.EndDisabledGroup();

            pos.x += pos.width;
            pos.width = 20;
            var isClicked = GUI.Button(pos, "", EditorStyles.popup);
            if (isClicked)
                ShowSearchWindow(property, attr);

        }

        void CheckEditorDisableGroupAttr(out bool isGroupDisabled)
        {
            isGroupDisabled = false;
            var groupAttrs = fieldInfo.GetCustomAttributes(typeof(EditorDisableGroupAttribute), false);
            if (groupAttrs != null && groupAttrs.Length > 0) // EditorGUI.indentLevel == 0 && 
            {
                if (groupAttrs[0] is EditorDisableGroupAttribute groupAttr)
                {
                    isGroupDisabled = groupAttr.isGroupDisabled;
                }
            }
        }

        private void CheckGroupAttr(ref bool isIndentAdd,ref bool isGroupOn)
        {
            isGroupOn = true; // default group is on

            var groupAttrs = fieldInfo.GetCustomAttributes(typeof(EditorGroupAttribute), false);
            if (groupAttrs != null && groupAttrs.Length > 0) // EditorGUI.indentLevel == 0 && 
            {
                if (groupAttrs[0] is EditorGroupAttribute groupAttr)
                {
                    //isIndentAdd1 = !groupAttr.isHeader;
                    isGroupOn = MaterialGroupTools.IsGroupOn(groupAttr.groupName);
                }
            }
        }

        public string[] GetNames(StringListSearchableAttribute attr)
        {
            if (attr.type != null && !string.IsNullOrEmpty(attr.staticMemberName))
            {
                return attr.type.GetMemberValue<string[]>(attr.staticMemberName, null, null);
            }
            // use enum names
            if (attr.enumType != null)
            {
                return Enum.GetNames(attr.enumType);
            }
            if (!string.IsNullOrEmpty(attr.names))
                return attr.names.Split(',');
            return null;
        }

        void ShowSearchWindow(SerializedProperty property, StringListSearchableAttribute attr)
        {
            string[] names = GetNames(attr);
            if (names == null)
                return;

            var provider = SearchWindowTools.CreateProvider<StringListSearchProvider>();
            provider.windowTitle = attr.label;
            provider.itemList = names.Select(n => (n, (object)n)).ToList();
            provider.onSelectedChanged = ((string name, object userData) infoItem) =>
            {
                //Debug.Log(property.propertyPath);

                property.UpdateProperty(() =>
                {
                    property.stringValue = (string)infoItem.userData;
                });
            };

            SearchWindowTools.OpenSearchWindow(provider);
        }
    }
}
#endif