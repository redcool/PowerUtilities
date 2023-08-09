using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PowerUtilities
{
    /// <summary>
    /// scene culling group profile
    /// one profile per scene
    /// </summary>
    [Serializable]
    public class CullingGroupSO : ScriptableObject
    {
        public List<CullingInfo> cullingInfos = new List<CullingInfo>();



    }
}
