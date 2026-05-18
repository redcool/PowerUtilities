using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = UnityEngine.Object;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Unity.Object extensions
    /// </summary>
    public static class UnityObjectEx
    {
        static Dictionary<Object, string> objNameDict = new();

        /// <summary>
        /// Get Unity Object's name from objNameDict
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetName(this Object obj)
        {
            return DictionaryTools.Get(objNameDict, obj, obj => obj?.name);
        }

        /// <summary>
        /// Unified Object.FindObjectsByType 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static T[] FindObjects<T>(bool includeInactive) where T : Object
        {
            var activeMode = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
#if UNITY_6000_4_OR_NEWER
            return Object.FindObjectsByType<T>(activeMode);
#else
            return Object.FindObjectsByType<T>(activeMode, FindObjectsSortMode.None);
#endif
        }
        /// <summary>
        /// Unified Object.FindObjectByType 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="includeInactive"></param>
        /// <returns></returns>
        public static T FindObject<T>(bool includeInactive) where T : Object
        {
            var objs = FindObjects<T>(includeInactive);
            return objs != null && objs.Length > 0 ? objs[0] : null;
        }
    }
}
