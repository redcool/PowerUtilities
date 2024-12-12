namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// show editor gui in disable mode
    /// </summary>
    public class EditorDisableGroupAttribute : PropertyAttribute
    {
        /// <summary>
        /// Show DisableGroup when empty,
        /// otherwise check targetPropName's intValue
        /// </summary>
        public string targetPropName;

        /// <summary>
        /// show gui in disableGroup when {targetPropName} is enabled
        /// </summary>
        public bool isRevertMode;

        /// <summary>
        /// line count,
        /// </summary>
        public int heightScale = 1;

    }
}
