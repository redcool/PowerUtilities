namespace PowerUtilities
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// show Enum in SearchWindow(like GraphicsFormat)
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumSearchableAttribute : PropertyAttribute
    {
        /// <summary>
        /// if empty will use FieldInfo.FieldType
        /// </summary>
        public Type enumType;

        /// <summary>
        /// read GraphicsFormat.txt or not
        /// </summary>
        public string textFileName = "GraphicsFormat.txt";

        public EnumSearchableAttribute(Type enumType=null)
        {
            this.enumType = enumType;
        }
    }
}