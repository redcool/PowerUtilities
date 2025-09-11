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
    /// Apply Preset for selected items
    /// </summary>
    public class ApplyPresetsToSelected
    {
        const string ROOT_MENU = "PowerUtilities/Presets/";
        const string APPLY_PRESET_TO_SELECTED = ROOT_MENU + "Apply Default Preset To Selected Folder";

        /// <summary>
        /// Apply default presets(Preset Manager config) to selected folder items
        /// </summary>
        [MenuItem(APPLY_PRESET_TO_SELECTED)]
        public static void ApplyPresetsToFolderItems()
        {
            var defaultPresets= PresetTools.GetAllDefaultPresets();

            Selection.GetFiltered<Object>(SelectionMode.DeepAssets).ForEach(obj =>
            {
                var imp = TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));

                var preset = Preset.GetDefaultPresetsForObject(imp).FirstOrDefault();
                if (preset != null && preset.CanBeAppliedTo(imp)) {
                    preset.ApplyTo(imp);
                }

                //var isDone = PresetTools.TryApplyDefaultPreset(imp, defaultPresets);
            });
        }
    }
}
#endif