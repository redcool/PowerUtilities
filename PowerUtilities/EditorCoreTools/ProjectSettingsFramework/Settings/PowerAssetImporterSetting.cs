#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// PowerAssetImporterSetting ,shown in Preferences
    /// 
    /// 1 define class attr (ProjectSettingGroup,SOAssetPath)
    /// 2 public variables will shown on ProjectSetting(preferences)
    /// </summary>
    [ProjectSettingGroup("PowerUtils/AssetImporterSettings")]
    [SOAssetPath("Assets/PowerUtilities/PowerAssetImporterSettings.asset")]
    public class PowerAssetImporterSetting : ScriptableObject
    {
        [Tooltip("remove prefix MixamoRig ")]
        public bool isRemoveMixamoRig;

    }
}
#endif