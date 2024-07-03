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
        public Type enumType;

        /// <summary>
        /// read GraphicsFormat.txt
        /// </summary>
        public bool isReadTextFile=true;

        public EnumSearchableAttribute(Type enumType=null)
        {
            this.enumType = enumType;
        }
    }
}