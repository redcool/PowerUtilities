#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;

    public class StringListSearchProvider : BaseSearchWindowProvider<(string,object)>
    {

        /// <summary>
        /// fill List<SearchTreeEntry> 
        /// </summary>
        public List<(string name,object userData)> itemList = new();

        //public List<(string name,object userData)> itemListTest = new ()
        //{
        //    new (){name="a/b/1",userData= 1 },
        //    new (){name="a/b/2",userData= 2 },
        //    new() { name = "a/1", userData = 3 },
        //};


        public List<SearchTreeEntry> Parse(List<(string name,object userData)> strList)
        {
            var sb = new StringBuilder();
            Dictionary<string, (string name, int levelId, bool isGroup,object userData)> groupNameIdDict = new();

            var list = new List<SearchTreeEntry>();
            // fill search window title
            list.Add(new SearchTreeGroupEntry(new GUIContent(windowTitle), 0));

            foreach (var infoItem in strList)
            {
                var strItem = infoItem.name;
                var items = strItem.SplitBy('/');

                for (int i = 0; i < items.Length; i++)
                {
                    var itemName = items[i];

                    // get key,
                    for (int j = 0; j <= i; j++)
                    {
                        sb.Append(items[j]);
                    }
                    groupNameIdDict[sb.ToString()] = (itemName, i + 1, i < items.Length - 1,infoItem.userData);
                    sb.Clear();
                }
            }

            foreach (var infoItem in groupNameIdDict.Values)
            {
                var entry = infoItem.isGroup ? new SearchTreeGroupEntry(new GUIContent(infoItem.name)) : new SearchTreeEntry(new GUIContent(infoItem.name));
                entry.level = infoItem.levelId;
                entry.userData = (infoItem.name, infoItem.userData);
                list.Add(entry);
            }
            return list;
        }

        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var list = Parse(itemList);
            return list;
        }

    }
}
#endif