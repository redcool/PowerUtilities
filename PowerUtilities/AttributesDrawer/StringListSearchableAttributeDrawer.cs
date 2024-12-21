#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    [CustomPropertyDrawer(typeof(StringListSearchableAttribute))]
    public class StringListSearchableAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as StringListSearchableAttribute;
            // check enumType
            if (attr.enumType == null && property.propertyType == SerializedPropertyType.Enum)
            {
                attr.enumType = fieldInfo.FieldType;
            }

            var pos = position;
            pos.width = position.width - 20;
            EditorGUI.PropertyField(pos, property, label);

            pos.x += pos.width;
            pos.width = 20;
            var isClicked = GUI.Button(pos, "+", EditorStyles.popup);
            if (isClicked)
                ShowSearchWindow(property, attr);

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
                Debug.Log(property.propertyPath);

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