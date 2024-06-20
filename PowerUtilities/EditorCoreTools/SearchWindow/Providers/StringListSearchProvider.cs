#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;

    public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        public List<string> items = new()
        {
            "a/b/1",
            "a/b/2",
            "a/1",
        };

        public Action<string> onSetIndex;


        public List<SearchTreeEntry> GetSearchTreeList(List<string> strList, string title = "No Title")
        {
            var sb = new StringBuilder();
            Dictionary<string, (string name, int id, bool isGroup)> groupNameIdDict = new();

            var list = new List<SearchTreeEntry>();
            // fill search window title
            list.Add(new SearchTreeGroupEntry(new GUIContent(title), 0));

            foreach (var strItem in strList)
            {
                var items = strItem.SplitBy('/');

                for (int i = 0; i < items.Length; i++)
                {
                    var itemName = items[i];

                    // get key,
                    for (int j = 0; j <= i; j++)
                    {
                        sb.Append(items[j]);
                    }
                    groupNameIdDict[sb.ToString()] = (itemName, i + 1, i < items.Length - 1);
                    sb.Clear();
                }
            }

            foreach (var infoItem in groupNameIdDict.Values)
            {
                var entry = infoItem.isGroup ? new SearchTreeGroupEntry(new GUIContent(infoItem.name)) : new SearchTreeEntry(new GUIContent(infoItem.name));
                entry.level = infoItem.id;
                entry.userData = infoItem.name;
                list.Add(entry);
            }
            return list;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = GetSearchTreeList(items);
            return list;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            onSetIndex?.Invoke((string)searchTreeEntry.userData);
            return true;
        }
    }
}
#endif