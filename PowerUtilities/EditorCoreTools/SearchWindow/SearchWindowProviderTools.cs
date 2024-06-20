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
    public static class SearchWindowProviderTools
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
    }
}
#endif