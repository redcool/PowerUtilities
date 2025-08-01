﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PowerUtilities
{
    public static class SearchWindowTools
    {

        public static void FillWithEnum(ref List<SearchTreeEntry> list, Type enumType)
        {
            var names = Enum.GetNames(enumType);
            var values = Enum.GetValues(enumType);

            list.Add(new SearchTreeGroupEntry(new GUIContent(enumType.Name)));
            
            //foreach (var name in names)
            for (int i = 0;i<values.Length;i++)
            {
                var name = names[i];
                var value = values.GetValue(i);
                list.Add(new SearchTreeEntry(new GUIContent(name, name))
                {
                    level = 1,
                    //userData = Enum.Parse(enumType, name)
                    userData = (int)value
                }) ;
            }
        }

        public static void OpenSearchWindow<T>(Vector2 screenPos,T provider) where T : BaseSearchWindowProvider
        {
            SearchWindow.Open(new SearchWindowContext(screenPos), provider);
        }

        /// <summary>
        /// show window in screenPos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="provider"></param>
        public static void OpenSearchWindow<T>(T provider) where T : BaseSearchWindowProvider
        {
            var pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            SearchWindow.Open(new SearchWindowContext(pos), provider);
        }

        public static T CreateProvider<T>() where T : BaseSearchWindowProvider
        {
            return ScriptableObject.CreateInstance<T>();
        }

        public static StringListSearchProvider CreateStringListProviderAndShowWin(string title, List<(string, object)> itemList, Action<(string, object)> onSelected)
        {
            var p = CreateProvider<StringListSearchProvider>();
            p.windowTitle = title;
            p.itemList = itemList;
            p.onSelectedChanged = onSelected;

            OpenSearchWindow(p);
            return p;
        }

        public static EnumSearchProvider CreateEnumProviderAndShowWin(SerializedProperty property, Type enumType,Action<int> onSelected)
        {
            var provider = CreateProvider<EnumSearchProvider>();
            provider.windowTitle = enumType.Name;
            //provider.textFileName = attr.textFileName;
            provider.onSelectedChanged = onSelected;
            provider.enumType = enumType;

            OpenSearchWindow(provider);
            return provider;
        }
    }
}
#endif