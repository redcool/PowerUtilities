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
    using Object = UnityEngine.Object;

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Tools/GUIDFinder")]
    [SOAssetPath("Assets/PowerUtilities/AssetGUIDFinder.asset")]
    public class AssetGUIDFinder : ScriptableObject
    {
        [HelpBox]
        public string helpBox = "Find object by GUID";

        [ListItemDraw("guid:,guid,asset:,asset,path:,path,fId:,fileId","40,.26,40,.2,40,.3,20,.1")]
        public List<AssetFileInfo> assetInfoList = new();

        [EditorButton(onClickCall = "OnFind")]
        public bool isFind;

        public void OnFind()
        {
            foreach (AssetFileInfo info in assetInfoList)
            {
                info.Clear();
                if (string.IsNullOrEmpty(info.guid))
                    continue;
#if UNITY_EDITOR
                info.path = AssetDatabase.GUIDToAssetPath(info.guid);
                info.asset = AssetDatabase.LoadAssetAtPath<Object>(info.path);
                if(info.asset)
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(info.asset, out var guid, out info.fileId);
#endif
            }
        }
    }
}
