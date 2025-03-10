#if UNITY_EDITOR
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AssetBundleTools))]
    public class AssetBundleToolsEditor : PowerEditor<AssetBundleTools>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(AssetBundleTools inst)
        {
            EditorGUILayout.HelpBox("AssetBundle Tools", MessageType.Info);
        }
    }

    [Serializable]
    public class AssetBundleInfo
    {
        public string name;
        public List<GameObject> objects;
    }

    /// <summary>
    /// Unity AssetBundleTools 
    /// </summary>
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Project/AssetBundleTools")]
    [SOAssetPath("Assets/PowerUtilities/AssetBundleTools.asset")]
    public class AssetBundleTools : ScriptableObject
    {
        [ListItemDraw("name:,name,objects:,objects", "200,200,100,")]
        public List<AssetBundleInfo> infoList = new();
    }
}
#endif