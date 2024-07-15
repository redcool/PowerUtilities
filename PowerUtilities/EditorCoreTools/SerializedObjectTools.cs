#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public static class SerializedObjectTools
    {
        public static Dictionary<Object, SerializedObject> soDict = new Dictionary<Object, SerializedObject>();

        /// <summary>
        /// Get SerializedObject with cached
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static SerializedObject GetObject(Object obj)
        {
            var so = DictionaryTools.Get(soDict,obj,obj=> new SerializedObject(obj));
            if (so.targetObject != obj)
                so = soDict[obj] = new SerializedObject(obj);
            return so;
        }

        /// <summary>
        /// Get a property from keyObj's SerializedObject,
        /// </summary>
        /// <param name="keyObj"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static SerializedProperty GetProperty(Object keyObj,string propName)
        {
            if (!keyObj || string.IsNullOrEmpty(propName))
                return default;

            var prop = GetObject(keyObj).FindProperty(propName);
            return prop;
        }
    }
}
#endif