using System;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// common sphere info
    /// </summary>
    [Serializable]
    public class CullingInfo
    {
        public float size;
        public Vector3 pos;

        /// <summary>
        /// Set field will not trigger visible reaction
        /// </summary>
        public bool isVisible = true;
        /// <summary>
        /// current distance level index
        /// </summary>
        public int distanceBands;

        public CullingInfo(Vector3 pos, float size = 2)
        {
            this.pos = pos;
            this.size = size;
        }
    }
}
