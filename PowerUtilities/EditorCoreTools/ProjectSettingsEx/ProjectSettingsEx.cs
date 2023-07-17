#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Project or (Proference) Settings
    /// </summary>
    static class ProjectSettingsEx
    {
        static GUIContent projectSettingContentInst = new GUIContent();

        [SettingsProvider]
        static SettingsProvider ShowSettings()
        {
            var provider = new SettingsProvider("PowerUtils/AssetImporterSettings", SettingsScope.User)
            {
                label = "Asset Importer settings",
                guiHandler = (searchContex) =>
                {
                    var settings = PowerAssetImporterSetting.GetSerializedInstance();

                    // iterate fields
                    var t = typeof(PowerAssetImporterSetting);
                    var normalFields = t.GetFields();

                    normalFields.ForEach(field =>
                    {
                        if (field.GetCustomAttribute<ProjectSetttingFieldAttribute>(true) == null)
                            return;

                        var prop = settings.FindProperty(field.Name);
                        EditorGUILayout.PropertyField(prop, EditorGUITools.TempContent(field.Name, inst: projectSettingContentInst));
                    });

                    settings.ApplyModifiedProperties();
                },
            };
            return provider;
        }
    }
}
#endif