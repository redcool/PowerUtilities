#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PowerUtilities
{
    public static class SearchWindowTools
    {

        public static void FillWithEnum(ref List<SearchTreeEntry> list, Type enumType)
        {
            var names = Enum.GetNames(enumType);
            list.Add(new SearchTreeGroupEntry(new GUIContent(enumType.Name)));

            foreach (var name in names)
            {
                list.Add(new SearchTreeEntry(new GUIContent(name, name))
                {
                    level = 1,
                    userData = Enum.Parse(enumType, name)
                });
            }
        }

        public static void OpenSearchWindow<T>(Vector2 screenPos,T provider) where T : BaseSearchWindowProvider
        {
            SearchWindow.Open(new SearchWindowContext(screenPos), provider);
        }

        public static void OpenSearchWindow<T>(T provider) where T : BaseSearchWindowProvider
        {
            var pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            SearchWindow.Open(new SearchWindowContext(pos), provider);
        }
    }
}
#endif