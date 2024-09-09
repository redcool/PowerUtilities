#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(Matrix4x4))]
[CustomPropertyDrawer(typeof(Vector4))]
public class Matrix4x4Drawer : PropertyDrawer
{

    readonly string[] COLUMN_NAMES = { "x", "y", "z", "w" };

    readonly GUIContent UI_PasteContent = new GUIContent("Paste", "paste last copy");

    // current copy
    static List<float> copiedItemList = new List<float>();

    int GetRowCount(SerializedProperty property)
        => property.propertyType == SerializedPropertyType.Vector4 ? 1 : 4;

    /**
 * vector,(x,y,z,w)
 * matrix4x4(e00,...,eij)
 * */
    string GetItemPropName(SerializedProperty property, int row, int col)
    => property.propertyType == SerializedPropertyType.Vector4 ? COLUMN_NAMES[col] : "e" + row + col;

    int GetItemLabelWidth(SerializedProperty property)
        => property.propertyType == SerializedPropertyType.Vector4 ? 12 : 24;

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
        DrawContext(position, property);

        // draw contexts
        DrawMatrixOrVector(ref position, property, label);
    }

    void DrawContext(Rect position, SerializedProperty property)
    {
        var contextPos = position;
        contextPos.height = EditorGUIUtility.singleLineHeight;

        var e = Event.current;
        if (e.IsMouseTrigger(EventType.MouseUp, 1) && contextPos.Contains(e.mousePosition))
        {
            DoContextMenu(property);
            e.Use();
        }
    }

    private void DrawMatrixOrVector(ref Rect position, SerializedProperty property, GUIContent label)
    {
        var rowCount = GetRowCount(property);
        var columnCount = 4; //

        position.height = EditorGUIUtility.singleLineHeight;
        EditorGUI.PropertyField(position, property, label, false);


        EditorGUITools.DrawBoxColors(position);

        if (!property.isExpanded)
            return;

        ++EditorGUI.indentLevel;
        position = EditorGUI.IndentedRect(position);
        --EditorGUI.indentLevel;

        //
        var labelWidth = GetItemLabelWidth(property);
        var itemPos = position;
        var itemWidth = (EditorGUIUtility.currentViewWidth - position.x / 2) / 4 - 6;

        //row
        for (int i = 0; i < rowCount; ++i)
        {
            itemPos.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            //column
            for (int j = 0; j < columnCount; ++j)
            {
                itemPos.x = position.x + j * itemWidth;

                var itemPropName = GetItemPropName(property, i, j);
                var itemProp = property.FindPropertyRelative(itemPropName);
                // label
                itemPos.width = labelWidth;
                EditorGUI.LabelField(itemPos, itemPropName);

                // content
                itemPos.x += labelWidth;
                itemPos.width = itemWidth - labelWidth;
                EditorGUI.PropertyField(itemPos, itemProp, GUIContent.none);
            }
        }
    }

    private void DoContextMenu(SerializedProperty property)
    {
        var menu = new GenericMenu();
        //======= copy
        SetupCopy(property, menu);

        //======= paste
        SetupPaste(property, menu);
        //======= tools zero
        menu.AddSeparator("--------");
        SetToZero(property, menu);

        //======= tools one
        SetToOne(property, menu);

        //======= matrix identity
        var isMatrix4x4 = property.IsMatrix4x4();
        if (isMatrix4x4)
        {
            menu.AddSeparator("--------");
            SetMatrixIdentity(property, menu);
        }

        menu.ShowAsContext();
    }

    private static void SetMatrixIdentity(SerializedProperty property, GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Identity", "items set Identity"), false, (propObj) =>
        {
            var prop = (SerializedProperty)propObj;
            prop.UpdateProperty(() =>
            {
                var arr = prop.GetEnumerable(isUseCopy: true).ToArray();
                foreach (var item in arr)
                {
                    item.floatValue = 0;
                }

                for (int i = 0; i < 4; i++)
                    arr[i + 4 * i].floatValue = 1;
            });
        }, property);
    }

    private static void SetToOne(SerializedProperty property, GenericMenu menu)
    {
        menu.AddItem(new GUIContent("One", "items set to one"), false, () =>
        {
            property.UpdateProperty(() =>
            {
                property.GetEnumerable().ForEach((itemProp, id) =>
                {
                    itemProp.floatValue = 1;
                });
            });
        });
    }

    private static void SetupCopy(SerializedProperty property, GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Copy", "copy values"), false, () =>
        {
            copiedItemList = property.GetEnumerable().Select(itemProp => itemProp.floatValue).ToList();
        });
    }

    private void SetupPaste(SerializedProperty property, GenericMenu menu)
    {
        var isItemCountValid = property.IsMatrix4x4() ? copiedItemList.Count == 16 : copiedItemList.Count == 4;
        if (copiedItemList.Count > 0 && isItemCountValid)
        {
            menu.AddItem(UI_PasteContent, false, () =>
            {
                if (copiedItemList.Count == 0)
                    return;

                property.UpdateProperty(() =>
                {
                    property.GetEnumerable().ForEach((itemProp, id) =>
                    {
                        itemProp.floatValue = copiedItemList[id];
                    });
                });
            });
        }
        else
        {
            menu.AddDisabledItem(UI_PasteContent);
        }
    }

    private static void SetToZero(SerializedProperty property, GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Zero", "items set to zero"), false, () =>
        {
            property.UpdateProperty(() =>
            {
                property.GetEnumerable().ForEach((itemProp, id) =>
                {
                    itemProp.floatValue = 0;
                });
            });
        });
    }
}
#endif