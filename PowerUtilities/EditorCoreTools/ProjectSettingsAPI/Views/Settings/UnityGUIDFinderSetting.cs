namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Tools/GUIDFinder")]
    [SOAssetPath("Assets/PowerUtilities/UnityGUIDFinderSetting.asset")]
    public class UnityGUIDFinderSetting : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "Find object by GUID";
        public string assetGUID;
        public string assetPath;

        [EditorButton(onClickCall = "OnFind")]
        public bool isFind;

        public void OnFind()
        {
            if (string.IsNullOrEmpty(assetGUID))
            return;

#if UNITY_EDITOR
            assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
#endif
        }
    }
}
