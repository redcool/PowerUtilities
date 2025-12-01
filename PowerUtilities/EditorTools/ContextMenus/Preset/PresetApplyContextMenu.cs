#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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
    public static class PresetApplyContextMenu
    {
        

        /// <summary>
        /// Apply preset in selected folder(
        /// </summary>
        [MenuItem(ContextMenuConsts.POWER_UTILS_MENU + "/Preset/Apply Preset In Folder Chain")]
        public static void ApplyPresetInFolderChain()
        {
            var selectedFolders = SelectionTools.GetSelectedFolders();
            foreach (var selectedFolder in selectedFolders)
            {
                var presetsList = PresetTools.GetPresetsInFolderChain(selectedFolder);
                var objs = AssetDatabaseTools.FindAssetsInProject<Object>("", selectedFolder)
                    .Where(obj => obj.GetType() != typeof(Preset))
                    ;

                foreach (var obj in objs)
                {
                    foreach (var presetInfo in presetsList)
                    {
                        if (PresetTools.TryApplyPreset(obj, presetInfo.presets))
                        {
                            Debug.Log($"{obj} apply preset in :{presetInfo.folder}");
                            break;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Apply default presets(Preset Manager config) to selected folder items
        /// </summary>
        [MenuItem(ContextMenuConsts.POWER_UTILS_MENU + "/Preset/Apply Default Presets In Folder")]
        public static void ApplyDefaultPresetInFolder()
        {
            var list = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            list.ForEach(obj =>
            {
                PresetTools.TryApplyDefaultPreset(obj);
            });
        }

    }
}
#endif