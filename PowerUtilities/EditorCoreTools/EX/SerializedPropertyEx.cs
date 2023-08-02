#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
public static class SerializedPropertyEx
{
    public static bool IsElementExists(this SerializedProperty arrayProp, Predicate<SerializedProperty> predicate)
    {
        return GetElementIndex(arrayProp, predicate) != -1;
    }

    public static int GetElementIndex(this SerializedProperty arrayProp,Predicate<SerializedProperty> predicate)
    {
        if (!arrayProp.isArray || predicate == null)
            return -1;

        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            if (predicate(arrayProp.GetArrayElementAtIndex(i)))
                return i;
        }
        return -1;
    }

    public static SerializedProperty AppendElement(this SerializedProperty arrayProp,Action<SerializedProperty> onFillContent=null)
    {
        if (!arrayProp.isArray)
            return default;

        var id = arrayProp.arraySize;
        arrayProp.InsertArrayElementAtIndex(id);
        var prop = arrayProp.GetArrayElementAtIndex(id);

        if (onFillContent != null)
        {
            onFillContent(prop);
        }
        return prop;
    }

    public static IEnumerable<SerializedProperty> GetElements(this SerializedProperty arrayProp)
    {
        if (!arrayProp.isArray)
            return Enumerable.Empty<SerializedProperty>();

        var elements = new List<SerializedProperty>();
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            elements.Add(arrayProp.GetArrayElementAtIndex(i));
        }
        return elements;
    }

}
#endif