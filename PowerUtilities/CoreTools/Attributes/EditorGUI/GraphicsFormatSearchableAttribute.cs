namespace PowerUtilities
{
    using Codice.Client.Common.FsNodeReaders;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// show GraphicsFormat in SearchWindow
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class GraphicsFormatSearchableAttribute : PropertyAttribute
    {
        /// <summary>
        /// read GraphicsFormat.txt
        /// </summary>
        public bool isReadTextFile=true;
    }
}