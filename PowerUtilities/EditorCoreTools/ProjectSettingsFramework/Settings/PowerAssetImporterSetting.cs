#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// a settings will be shown in Preferences
    /// </summary>
    [ProjectSettingGroup("PowerUtils/AssetImporterSettings")]
    [SOAssetPath("Assets/PowerUtilities/PowerAssetImporterSettings.asset")]
    public class PowerAssetImporterSetting : ScriptableObject
    {

        [ProjectSetttingField]
        public bool isRemoveMixamoRig;

    }
}
#endif