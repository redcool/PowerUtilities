namespace PowerUtilities
{
    using Codice.Client.Common.FsNodeReaders;
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

        public EnumSearchableAttribute(Type enumType)
        {
            this.enumType = enumType;
        }
    }
}