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
    public static class PresetTools
    {
        /// <summary>
        /// Get all defaultPresets by defaultTypes
        /// </summary>
        /// <returns></returns>
        public static DefaultPreset[] GetAllDefaultPresets()
        {
            var types = Preset.GetAllDefaultTypes();
            return types.SelectMany(t => Preset.GetDefaultPresetsForType(t))
                .ToArray();
        }
        /// <summary>
        /// First try apply AssetImporter 
        /// Then try apply to obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool TryApplyDefaultPreset(Object obj)
        {
            var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
            if (imp.IsAssetImporter())
                return ApplyFirstDefaultPreset(imp);
            return ApplyFirstDefaultPreset(obj);
        }
        /// <summary>
        /// Try apply first default preset 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ApplyFirstDefaultPreset(Object obj)
        {
            var preset = Preset.GetDefaultPresetsForObject(obj).FirstOrDefault();
            if (preset != null && preset.CanBeAppliedTo(obj))
            {
                return preset.ApplyTo(obj);
            }
            return false;
        }

        /// <summary>
        /// First try apply AssetImporter(iterate presets)
        /// Then try apply to obj
        /// </summary>
        /// <param name="presets"></param>
        /// <param name="obj"></param>
        public static bool TryApplyPreset(Object obj, params Preset[] presets)
        {
            var imp = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(obj));
            
            if (imp.IsAssetImporter())
                return ApplyPreset(imp, presets);
            return ApplyPreset(obj, presets);
        }

        /// <summary>
        /// Try apply Object (try presets from first to end)
        /// </summary>
        /// <param name="presets"></param>
        /// <param name="obj"></param>
        public static bool ApplyPreset(Object obj, params Preset[] presets)
        {
            if (obj == null || presets == null || presets.Length == 0)
                return false;

            foreach (var preset in presets)
            {
                if (preset.CanBeAppliedTo(obj))
                {
                    return preset.ApplyTo(obj);
                }
            }
            return false;
        }

        /// <summary>
        /// Get preset in folder chain.{assetFolder, parent folder, ...}
        /// </summary>
        /// <param name="assetFolder"></param>
        /// <returns>(string folder,Preset[] presets)</returns>
        public static List<(string folder,Preset[] presets)> GetPresetsInFolderChain(string assetFolder)
        {
            var folders = PathTools.GetParentFolders(assetFolder);

            var list = new List<(string,Preset[])>();
            foreach (var folder in folders)
            {
                var presets = AssetDatabaseTools.FindAssetsInFolder<Preset>("", folder);
                list.Add((folder, presets));
            }
            return list;
        }
    }
}
#endif