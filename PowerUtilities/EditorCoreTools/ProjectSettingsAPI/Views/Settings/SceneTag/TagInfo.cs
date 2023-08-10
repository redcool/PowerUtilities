namespace PowerUtilities
{
    using System;
    using UnityEngine;

    [Serializable]
    public class TagInfo
    {
        /// <summary>
        /// object 's tag
        /// </summary>
        public string tag;

        /// <summary>
        /// object material's renderQueue
        /// </summary>
        public int renderQueue = 2000;

        /// <summary>
        /// material's keywords
        /// </summary>
        [Tooltip("keywords,split by ,")]
        public string keywords="";
        public override string ToString()
        {
            return $"{tag},q:{renderQueue},kw:{keywords.Length}";
        }
    }
}
