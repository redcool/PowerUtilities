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

        static Dictionary<string, CullingGroupSO> cameraCullingDict = new Dictionary<string, CullingGroupSO>();
        public static CullingGroupSO GetProfle(string scenePath = "")
        {
            if (string.IsNullOrEmpty(scenePath))
                scenePath = SceneManager.GetActiveScene().path;

            if (cameraCullingDict.TryGetValue(scenePath, out var profile))
            {
                return profile;
            }
#if UNITY_EDITOR
            var path = $"{AssetDatabaseTools.CreateGetSceneFolder()}/CullingProfile.asset";
            profile = cameraCullingDict[scenePath] =  ScriptableObjectTools.CreateGetInstance<CullingGroupSO>(path);
#endif
            return profile;
        }

        public static CullingGroupSO SceneProfile
        {
            get { return GetProfle(); }
        }
    }
}
