﻿namespace PowerUtilities.Drawers
{
    using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;

    [CustomPropertyDrawer(typeof(EditorGroupAttribute))]
    public class EditorGroupDrawer : PropertyDrawer
    {
        (string groupName, bool isOn) groupInfo;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var groupAttr = attribute as EditorGroupAttribute;
            if (groupAttr.isHeader)
            {
                return 4;
                //return base.GetPropertyHeight(property, label);
            }

            //if (MaterialGroupTools.IsGroupOn(groupAttr.groupName))
            //{
            //    return 0;
            //}
            return 0;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var groupAttr = attribute as EditorGroupAttribute;

            groupInfo.groupName = groupAttr.groupName;
            groupInfo.isOn = MaterialGroupTools.IsGroupOn(groupAttr.groupName);

            //draw header
            if (groupAttr.isHeader)
            {
                EditorGUITools.DrawFoldContent(ref groupInfo, () =>
                {

                });
                //groupInfo.isOn = EditorGUILayout.Foldout(groupInfo.isOn, groupInfo.groupName);
                MaterialGroupTools.GroupDict[groupInfo.groupName] = groupInfo.isOn;
            }

            //draw contents
            if (!groupInfo.isOn)
                return;

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(property,new GUIContent(property.displayName));
            EditorGUI.indentLevel--;
        }
    }
#endif


}