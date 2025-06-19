using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{

    /// <summary>
    /// Show string list in Searchable Window
    /// 
    /// like EnumSearchableAttribute but use StringListSearchProvider
    /// 
    /// * must on top of EditorDisableGroupAttribute
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StringListSearchableAttribute : PropertyAttribute
    {
        /// <summary>
        ///1 get string list from enumType.names
        /// </summary>
        public Type enumType;

        /// <summary>
        ///2 get string list call targetType's targetTypeMemberName
        /// </summary>
        public Type type;
        public string staticMemberName;

        /// <summary>
        ///3 a string splited by ','
        /// </summary>
        public string names;

        public string label = "string list";
    }
}
