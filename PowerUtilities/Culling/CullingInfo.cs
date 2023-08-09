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
        public float size = 2;
        public Vector3 pos;
        public BoundingSphere boundingSphere;

        public CullingInfo(Vector3 pos, float size = 2)
        {
            boundingSphere = new BoundingSphere(pos, size);
        }
    }
}
