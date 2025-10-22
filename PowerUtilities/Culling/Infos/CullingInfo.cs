using System;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// culling sphere info
    /// </summary>
    [Serializable]
    public class CullingInfo
    {
        /// <summary>
        /// sphere radius
        /// </summary>
        public float size;
        /// <summary>
        /// sphere center
        /// </summary>
        public Vector3 pos;

        /// <summary>
        /// Set field will not trigger visible reaction
        /// </summary>
        public bool isVisible = true;
        /// <summary>
        /// current distance level index,use for lod
        /// </summary>
        public int distanceBands;

        public CullingInfo(Vector3 pos, float size = 2)
        {
            this.pos = pos;
            this.size = size;
        }
    }
}
