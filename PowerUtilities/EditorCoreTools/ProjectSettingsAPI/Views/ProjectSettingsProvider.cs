#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Show Project or (Proference) Settings
    /// </summary>
    public static class ProjectSettingsProvider
    {
        public readonly static GUIContent projectSettingContentInst = new GUIContent();
        static CacheTool<Type, Editor> cachedEditors = new CacheTool<Type, Editor>();

        [SettingsProviderGroup]
        public static SettingsProvider[] CreateProviders()
        {
            var list = new List<SettingsProvider>();
            var settingTypes = TypeCache.GetTypesWithAttribute<ProjectSettingGroupAttribute>();
            foreach (var settingType in settingTypes)
            { 
                var groupAttr = settingType.GetCustomAttribute<ProjectSettingGroupAttribute>();
                if (groupAttr == null)
                    continue;

                var sp = new SettingsProvider(groupAttr.settingPath, (SettingsScope)groupAttr.settingsScope)
                {
                    label = groupAttr.label,
                    guiHandler = (searchContex) =>
                    {
                        ShowSetting(settingType);
                    },
                };

                list.Add(sp);
            }
            return list.ToArray();
        }


        public static void ShowSetting(Type type)
        {
            var setting = ScriptableObjectTools.GetSerializedInstance(type);
            if (setting.targetObject == null)
            {
                EditorGUILayout.SelectableLabel($"{type} cannot load.");
                return;
            }
            // show so object 
            EditorGUITools.BeginHorizontalBox(() => { 
                EditorGUILayout.LabelField("SO:",GUILayout.Width(148));
                EditorGUILayout.ObjectField(setting.targetObject, typeof(ScriptableObject), false);
            });
            
            EditorGUITools.DrawColorLine();

            // show so properties
            var settingEditor = cachedEditors.Get(type, () => Editor.CreateEditor(setting.targetObject));
            settingEditor.OnInspectorGUI();
            setting.ApplyModifiedProperties();
        }
    }
}
#endif