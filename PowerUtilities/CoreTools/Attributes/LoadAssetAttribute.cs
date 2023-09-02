namespace PowerUtilities
{
    using Codice.Client.BaseCommands;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Load Asset when show in inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class LoadAssetAttribute : PropertyAttribute
    {
        /// <summary>
        /// assetPath : start with Assets
        /// relativePath :exclude Assets, need extends name, like xxx.shader
        /// </summary>
        public string assetPathOrName;

        /// <summary>
        /// start path from Asset
        /// </summary>
        public string searchInFolder;

        public LoadAssetAttribute(string assetPath, string searchInFolder = "")
        {
            this.assetPathOrName = assetPath;
            this.searchInFolder = searchInFolder;

        }
    }
}
