namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    public class EditorSceneViewAttribute : PropertyAttribute
    {
        /// <summary>
        /// item container's type
        /// </summary>
        public Type containerType;
        public EditorSceneViewAttribute()
        {
        }
    }
}
