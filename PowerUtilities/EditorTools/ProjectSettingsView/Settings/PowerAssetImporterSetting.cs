#if UNITY_EDITOR
using System.IO;
using UnityEditor;
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
        //---------------
        [EditorHeader("","objects rename")]
        public string nameReplaced;
        public string newName;

        [Tooltip("rename selected objects(hierarchy)")]
        [EditorButton(onClickCall = "RenameSelectedObjects")]
        public bool isRenameSelectedObjects;

        [Space(20)]
        //---------------
        [EditorHeader("","MixamoRename")]
        [Tooltip("remove prefix MixamoRig ")]
        [EditorButton(onClickCall = "RemoveMixamoPrefix",text = "RemoveMixamoPrefix")]
        public bool isRemoveMixamoRig;

        [Tooltip("enable MIXAMO_RENAME, unity will call fbx import flow")]
        [EditorButton(onClickCall = "EnableMixamoAssetImportFlow",text = "EnableMixamoAssetImportFlow")]
        public bool isEnableMixamoAssetImportFlow;

        void RemoveMixamoPrefix()
        {
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                go.RenameHierarchy(@"mixamorig\d*:", "");
            }
        }

        void RenameSelectedObjects()
        {
            var gos = Selection.gameObjects;
            foreach (var go in gos)
            {
                go.RenameHierarchy(nameReplaced, newName);
            }
        }

        void EnableMixamoAssetImportFlow()
        {
            PlayerSettingTools.AddMacroDefines("MIXAMO_RENAME");
        }
    }
}
#endif