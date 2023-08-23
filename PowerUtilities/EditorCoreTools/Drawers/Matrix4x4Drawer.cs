#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Matrix4x4))]
public class Matrix4x4Drawer : PropertyDrawer
{

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
            return h;

        h *= 5;
        var vh = EditorGUIUtility.standardVerticalSpacing;
        h += vh * 4;
        return h;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, property, label, false);

        var e = Event.current; 
        if(e.IsmouseRightButton() && position.Contains(e.mousePosition) )
        {
            //DoContextMenu(property);
            //e.Use();
        }
        EditorGUITools.DrawBoxColors(position);

        if (!property.isExpanded)
            return;

        ++EditorGUI.indentLevel;
        position = EditorGUI.IndentedRect(position);
        --EditorGUI.indentLevel;

        var itemPos = position;
        itemPos.width = (EditorGUIUtility.currentViewWidth-position.x/2)/4 - 6;
        //row
        for (int i = 0; i < 4; ++i)
        {
            itemPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //column
            for (int j = 0; j < 4; ++j)
            {
                itemPos.x = position.x + j * itemPos.width + 2;
                var itemProp = property.FindPropertyRelative("e"+i+j);
                EditorGUI.PropertyField(itemPos, itemProp, GUIContent.none);
            }
        }
    }

    private void DoContextMenu(SerializedProperty property)
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("test", "test menu"), false, () => {
            Debug.Log("on test click");
        });

        menu.ShowAsContext();
    }
}
#endif