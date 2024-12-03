using System;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Relation rt1,rt2
    /// </summary>
    [Serializable]
    public class RebingTargetNameInfo
    {
        public string originalRTName, otherRTName;

        [Tooltip("draw objects in gamma or linear")]
        public ColorSpaceTransform.ColorSpaceMode colorSpace;

        public bool IsValid() => !string.IsNullOrEmpty(originalRTName) && !string.IsNullOrEmpty(otherRTName);
    }
}
