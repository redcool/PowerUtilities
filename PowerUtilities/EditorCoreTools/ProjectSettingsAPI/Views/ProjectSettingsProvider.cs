#if UNITY_EDITOR
using PowerUtilities.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
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

        [CompileFinished]
        static void OnCompileFinished()
        {
            cachedEditors.Clear();
        }

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
                        //EditorGUIUtility.labelWidth = 250;// EditorGUIUtility.currentViewWidth * groupAttr.labelWidthRate;
                        DrawSettingGUI(settingType, null);
                    },
                };
                // use UIElements
                if (groupAttr.isUseUIElment)
                {
                    //sp.guiHandler = null;
                    sp.activateHandler = (searchContex, rootElement) =>
                    {

                        DrawSettingGUI(settingType, rootElement);
                    };
                }

                list.Add(sp);
            }
            return list.ToArray();
        }


        public static void DrawSettingGUI(Type type, VisualElement rootElement)
        {
            var setting = ScriptableObjectTools.GetSerializedInstance(type);
            if (setting.targetObject == null)
            {
                EditorGUILayout.SelectableLabel($"{type} cannot load.");
                return;
            }
            setting.UpdateIfRequiredOrScript();

            var settingEditor = cachedEditors.Get(type, () => Editor.CreateEditor(setting.targetObject));
            // show so properties
            if (rootElement != null)
            {
                DrawSettingUIElements(type, rootElement, setting, settingEditor);
            }
            else
            {
                DrawSettingSO_SplitLine(setting);
                settingEditor.OnInspectorGUI();
            }

            setting.ApplyModifiedProperties();

        }

        private static void DrawSettingUIElements(Type type, VisualElement rootElement, SerializedObject setting, Editor settingEditor)
        {
            // add bold title
            var title = new Label(type.Name);
            title.style.fontSize = 20;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            rootElement.Add(title);

            // add content
            rootElement.Add(new IMGUIContainer(() =>
            {
                DrawSettingSO_SplitLine(setting);
                // show script
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    EditorGUILayout.PropertyField(setting.FindProperty("m_Script"));
                }
            }));
            rootElement.Add(settingEditor.CreateInspectorGUI());
        }

        /// <summary>
        /// Draw SettingSO (property) and split line
        /// </summary>
        /// <param name="setting"></param>
        public static void DrawSettingSO_SplitLine(SerializedObject setting)
        {
            // show so object 
            EditorGUITools.BeginHorizontalBox(() =>
            {
                GUILayout.Label("SO:");
                EditorGUILayout.ObjectField(setting.targetObject, typeof(ScriptableObject), false);
            });

            EditorGUIUtility.labelWidth = GUILayoutUtility.GetLastRect().width * 0.415F;

            // draw splitter 
            EditorGUITools.DrawColorLine();
        }
    }
}
#endif