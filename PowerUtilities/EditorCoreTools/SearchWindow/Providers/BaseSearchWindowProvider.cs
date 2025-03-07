﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace PowerUtilities
{
    public abstract class BaseSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);
        public abstract bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context);
    }
    /// <summary>
    /// SearchWindow common ops
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSearchWindowProvider<T> : BaseSearchWindowProvider
    {
        public string windowTitle = "No Title";
        /// <summary>
        /// call when item selected in SearchWindow
        /// </summary>
        public Action<T> onSelectedChanged;

        //public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);

        public override bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //Debug.Log(SearchTreeEntry.userData +"  : "+typeof(T) +":"+ SearchTreeEntry.userData.GetType());
            onSelectedChanged?.Invoke((T)searchTreeEntry.userData);
            return true;
        }
    }
}
#endif