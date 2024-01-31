namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Load Asset when show in inspector,target field is null will try find assetPathOrName
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LoadAssetAttribute : PropertyAttribute
    {
        /// <summary>
        /// assetPathOrName : start with Assets
        /// relativePath :exclude Assets, need extends name, like xxx.shader
        /// </summary>
        public string assetPathOrName;

        /// <summary>
        /// start path from Asset
        /// </summary>
        public string searchInFolder;

        public LoadAssetAttribute(string assetPathOrName, string searchInFolder = "")
        {
            this.assetPathOrName = assetPathOrName;
            this.searchInFolder = searchInFolder;

        }
    }
}
