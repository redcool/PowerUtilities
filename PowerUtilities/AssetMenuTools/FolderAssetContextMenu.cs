#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Unity Folder context menu(right click )
    /// use first preset
    /// </summary>
    public static class FolderAssetContextMenu
    {
        public const string ASSET_POWER_TOOLS_MENU = "Assets/PowerTools";


        [MenuItem(ASSET_POWER_TOOLS_MENU+"/Apply Preset In Folder")]
        public static void ApplyPresetInFolder()
        {
            var selectedFolders = SelectionTools.GetSelectedFolders();
            var objs = AssetDatabaseTools.FindAssetsInProject<Object>("", selectedFolders);
            var presets = AssetDatabaseTools.FindAssetsInProject<Preset>("", selectedFolders);
            var groups = presets
                .GroupBy(preset => preset.GetPresetType())
                .Select(pg => pg.FirstOrDefault());

            foreach (var preset in groups)
            {
                //PresetTools.TryApplyDefaultPreset()
            }

        }

        /// <summary>
        /// Apply default presets(Preset Manager config) to selected folder items
        /// </summary>
        [MenuItem(ASSET_POWER_TOOLS_MENU + "/Apply Default Presets In Folder")]
        public static void ApplyDefaultPresetInFolder() {
            ApplyPresetsToSelected.ApplyPresetsToFolderItems();
        }
    }
}
#endif