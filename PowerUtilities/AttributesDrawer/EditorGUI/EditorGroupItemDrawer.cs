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
    [CustomPropertyDrawer(typeof(EditorGroupItemAttribute))]
    public class EditorGroupItemDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorGroupItemAttribute;

            var isGroupOn = MaterialGroupTools.IsGroupOn(attr.groupName);
            if (isGroupOn)
                return EditorGUI.GetPropertyHeight(property,true);
            return -2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = attribute as EditorGroupItemAttribute;

            var isGroupOn = MaterialGroupTools.IsGroupOn(attr.groupName);
            if (!isGroupOn)
                return;

            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property, label,true);
            EditorGUI.indentLevel--;
        }
    }
}
#endif