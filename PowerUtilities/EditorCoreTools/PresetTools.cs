#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Presets;
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
        /// Try apply first default preset to object
        /// </summary>
        /// <param name="presets"></param>
        /// <param name="obj"></param>
        public static bool TryApplyDefaultPreset(Object obj, params DefaultPreset[] presets)
        {
            if (obj == null)
                return false;

            if(presets == null || presets.Length == 0)
                presets = GetAllDefaultPresets();

            foreach (var defaultPreset in presets)
            {
                if (defaultPreset.enabled )
                {
                    if (defaultPreset.preset.ApplyTo(obj))
                        return true;
                }
            }
            return false;
        }
    }
}
#endif