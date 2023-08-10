using System;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// DrawChildrenInstanced cullingInfo
    /// </summary>
    [Serializable]
    public class InstancedGroupCullingInfo : CullingInfo
    {
        /*
        group1:
        {
            transformGroup1{transform id}, {}
        },
        group2:{ ... }
         */
        public int groupId; // vertical group id
        public int transformGroupId; // horizontal group(segment) id
        public int transformId; // horizontal id
        public InstancedGroupCullingInfo(Vector3 pos, float size = 2) : base(pos, size)
        {
        }

        public override string ToString()
        {
            return $"{{{groupId},{transformGroupId},{transformId}}}";
        }
    }
}
