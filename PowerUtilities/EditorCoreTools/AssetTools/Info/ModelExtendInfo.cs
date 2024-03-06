namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Mesh extend info (uv2List
    /// </summary>
    [Serializable]
    public class ModelExtendInfo : ScriptableObject
    {
        public string assetPath;
        public List<Mesh> meshList = new List<Mesh>();
    }

}
