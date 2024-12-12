#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
public static class SerializedPropertyEx
{
    public static bool IsElementExists(this SerializedProperty arrayProp, Predicate<SerializedProperty> predicate)
    {
        return GetElementIndex(arrayProp, predicate) != -1;
    }

    public static int GetElementIndex(this SerializedProperty arrayProp, Predicate<SerializedProperty> predicate)
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

    public static SerializedProperty AppendElement(this SerializedProperty arrayProp, Action<SerializedProperty> onFillContent = null)
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

    /// <summary>
    /// get array 's items
    /// </summary>
    /// <param name="arrayProp"></param>
    /// <returns></returns>
    public static List<SerializedProperty> GetElements(this SerializedProperty arrayProp, bool isRemoveDuplicat = false)
    {
        if (!arrayProp.isArray)
            return new List<SerializedProperty>();

        var elements = new List<SerializedProperty>();

        if (isRemoveDuplicat)
        {
            AddElementsRemoveDuplicate(arrayProp, ref elements);
        }
        else
        {
            AddElements(arrayProp, elements);
        }

        return elements;

        //------ inner methods
        static void AddElementsRemoveDuplicate(SerializedProperty arrayProp, ref List<SerializedProperty> elements)
        {
            var set = new HashSet<object>();
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                var prop = arrayProp.GetArrayElementAtIndex(i);
                if (set.Contains(prop.objectReferenceValue))
                    continue;

                set.Add(prop.objectReferenceValue);
                elements.Add(prop);
            }
        }

        static void AddElements(SerializedProperty arrayProp, List<SerializedProperty> elements)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                elements.Add(arrayProp.GetArrayElementAtIndex(i));
            }
        }
    }

    public static void RemoveDuplicateItems(this SerializedProperty arrayProp)
    {
        if (!arrayProp.isArray)
            return;

        var set = new HashSet<object>();
        for (int i = arrayProp.arraySize - 1; i >= 0; i--)
        {
            var prop = arrayProp.GetArrayElementAtIndex(i);
            if (set.Contains(prop.objectReferenceValue))
            {
                arrayProp.DeleteArrayElementAtIndex(i);
                continue;
            }

            set.Add(prop.objectReferenceValue);
        }
    }

    /// <summary>
    /// Get property(Object's type) when cachedType is null
    /// 
    /// </summary>
    /// <param name="property"></param>
    /// <param name="cachedType"></param>
    //public static void TryGetType(this SerializedProperty property, ref Type cachedType)
    //{
    //    if (cachedType != null)
    //        return;

    //    if (property.objectReferenceValue != null)
    //        cachedType = property.objectReferenceValue.GetType();
    //    else
    //    {
    //        var typeName = property.type.Replace("PPtr<$", "").SubstringLast(1);
    //        cachedType = AssemblyTools.GetType(typeName);
    //    }
    //}

    /// <summary>
    /// Enumerate property's children properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="p"></param>
    /// <param name="onItemConvert"></param>
    /// /// <param name="isResetToFirst">point to first item, when enumerate done</param>
    /// <param name="isUseCopy">copy item property, ToArray(ToList) need this</param>
    /// /// <param name="onItemConvert"></param>
    /// <returns></returns>
    public static IEnumerable<SerializedProperty> GetEnumerable(this SerializedProperty property, bool isResetToFirst = true,bool isUseCopy=false)
    {
        var firstProp = property.Copy();

        //if (property.isArray)
        //{
        //    for (int i = 0; i < property.arraySize; i++)
        //    {
        //        yield return property.GetArrayElementAtIndex(i);
        //    }
        //}
        //else
        //{
        //    SerializedProperty end = property.GetEndProperty();
        //    while (property.NextVisible(enterChildren: true) && !SerializedProperty.EqualContents(property, end))
        //    {
        //        yield return property;
        //    }
        //    property = firstProp;
        //}

        foreach (SerializedProperty item in property)
        {
            yield return isUseCopy ? item.Copy() : item;
        }

        if (isResetToFirst)
            property = firstProp;
    }

    public static Matrix4x4 GetMatrix(this SerializedProperty p)
    {
        var m = Matrix4x4.zero;
        var id = 0;
        foreach (SerializedProperty item in p)
        {
            m[id++] = item.floatValue;
        }
        return m;
    }
    public static void SetMatrix(this SerializedProperty p, Matrix4x4 value)
    {
        var id = 0;
        foreach (SerializedProperty item in p)
        {
            item.floatValue = value[id++];
        }
    }
    /// <summary>
    /// Is 4x4 matrix ?
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool IsMatrix4x4(this SerializedProperty p)
    => p.type.Contains("Matrix4x4f");

    /// <summary>
    /// Update property then apply
    /// </summary>
    /// <param name="p"></param>
    /// <param name="onUpdate"></param>
    public static void UpdateProperty(this SerializedProperty p, Action onUpdate)
    {
        if (onUpdate == null || p.serializedObject == null)
            return;

        p.serializedObject.Update();
        onUpdate();
        p.serializedObject.ApplyModifiedProperties();
    }
    /// <summary>
    /// Is property is object, like xxx.yyy, xx.Array.data[0]
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    public static bool IsObject(this SerializedProperty property)
        // xxx.yyy, Array.data[0]
    => property.propertyPath.Contains(".");

    /// <summary>
    /// Update property's boxed value, 
    /// </summary>
    /// <param name="property"></param>
    /// <param name="onUpdate">object</param>
    public static void UpdatePropertyValue(this SerializedProperty property, Action<object> onUpdate)
    {
        /**
          property.propertyPath (array like):
              colorTargetInfos.Array.data[0].isFastSetFormat1

          get current item
              colorTargetInfos.Array.data[0]
       */
        if (property.IsObject())
        {
            UpdateArrayItemOrObject(property, onUpdate);
        }
        else
        {
            UpdateObject(property, onUpdate);
        }

        // object, can update 
        static void UpdateArrayItemOrObject(SerializedProperty property, Action<object> onUpdate)
        {
            var arrItemPropPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
            var arrItemProp = property.serializedObject.FindProperty(arrItemPropPath);

            var instObj = arrItemProp.boxedValue;
            onUpdate?.Invoke(instObj);
            // reset again
            arrItemProp.boxedValue = instObj;
        }

        // property.serializedObject,but cannot update serializedObject
        static void UpdateObject(SerializedProperty property, Action<object> onUpdate)
        {
            var instObj = property.serializedObject.targetObject;
            onUpdate?.Invoke(instObj);
        }
    }
    /// <summary>
    /// Demo:
    /// propertyPath : colorTargetInfos.Array.data[0].rtSizeMode
    /// return colorTargetInfos.Array.data[0]
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static string GetPropertyObjectPath(this SerializedProperty prop)
    {
        var path = prop.propertyPath;
        if (path.Contains("."))
            return path.Substring(0,path.LastIndexOf("."));
        return path;
    }
    /// <summary>
    /// Find property in current object
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    public static SerializedProperty FindPropertyInObject(this SerializedProperty prop,string propName)
    {
        var objPath = prop.GetPropertyObjectPath();
        return prop.serializedObject.FindProperty($"{objPath}.{propName}");
    }
}
#endif