#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Matrix4x4))]
[CustomPropertyDrawer(typeof(Vector4))]
public class Matrix4x4Drawer : PropertyDrawer
{

    readonly string[] COLUMN_NAMES = { "x", "y", "z", "w" };


    int GetRowCount(SerializedProperty property)
        => property.propertyType == SerializedPropertyType.Vector4 ? 1 : 4;

        /**
     * vector,(x,y,z,w)
     * matrix4x4(e00,...,eij)
     * */
    string GetItemPropName(SerializedProperty property, int row, int col)
    => property.propertyType == SerializedPropertyType.Vector4 ? COLUMN_NAMES[col] : "e" + row + col;



    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var rowCount = GetRowCount(property);

        float h = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
            return h;

        h *= (rowCount + 1);
        var vh = EditorGUIUtility.standardVerticalSpacing;
        h += vh * rowCount;
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rowCount = GetRowCount(property);
        var columnCount = 4; //

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
        for (int i = 0; i < rowCount; ++i)
        {
            itemPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //column
            for (int j = 0; j < columnCount; ++j)
            {
                itemPos.x = position.x + j * itemPos.width + 2;

                var itemPropName = GetItemPropName(property, i, j);
                var itemProp = property.FindPropertyRelative(itemPropName);
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