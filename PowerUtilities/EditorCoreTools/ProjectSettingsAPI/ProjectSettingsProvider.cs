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

                var sp = new SettingsProvider(groupAttr.settingPath, groupAttr.setingScope)
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
            var settingEditor = cachedEditors.Get(type, () => Editor.CreateEditor(setting.targetObject));
            settingEditor.OnInspectorGUI();

            // iterate fields
            //var normalFields = type.GetFields();

            //normalFields.ForEach(field =>
            //{
            //    if (field.GetCustomAttribute<HideInInspector>(true) != null)
            //        return;

            //    var prop = setting.FindProperty(field.Name);
            //    EditorGUILayout.PropertyField(prop, EditorGUITools.TempContent(field.Name, inst: projectSettingContentInst));
            //});

            setting.ApplyModifiedProperties();
        }
    }
}
#endif