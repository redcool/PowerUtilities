namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    /// <summary>
    /// Show vector3 in SceneView
    /// </summary>
    public class ShowInSceneViewAttribute : PropertyAttribute
    {
        /// <summary>
        /// item container's type
        /// </summary>
        public Type containerType;
        public ShowInSceneViewAttribute()
        {
        }
    }
}
