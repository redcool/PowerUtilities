#if UNITY_EDITOR
namespace PowerUtilities
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Unity AssetBundleEditorTools 
    /// </summary>
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/AssetBundleEditorTool",isUseUIElment =true)]
    [SOAssetPath("Assets/PowerUtilities/AssetBundleEditorTool.asset")]
    public class AssetBundleEditorTool : ScriptableObject
    {
        public string uxmlName = "AssetBundleEditorTool_Layout.uxml";
        public List<AssetBundleInfo> infoList = new();
    }
}
#endif