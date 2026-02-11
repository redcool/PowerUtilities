#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// get unity projectSetting folder/s asset,
    /// like ProjectSettings/TagManager.asset
    /// </summary>
    public static class ProjectSettingManagers
    {
        public enum ProjectSettingTypes
        {
            AudioManager,
            EditorSettings,
            ProjectSettings,
            QualitySettings,
            TagManager,
            URPProjectSettings,
            GraphicsSettings,
            TimelineSettings,
            PackageManagerSettings,
            ShaderGraphSettings,
            HDRPProjectSettings,
            VFXManager,
            PresetManager,
            EditorBuildSettings,
            InputManager,
            AutoStreamingSettings,
            MemorySettings,
            VersionControlSettings,
            ClusterInputManager,
            UnityConnectSettings,
            Physics2DSettings,
            NavMeshAreas,
            DynamicsManager,
            TimeManager,
        }
        public enum InputMode
        {
            OldInputManager = 0,
            NewInputSystem = 1,
            Both = 2,
        }
        /// <summary>
        /// Get file path that in folder ProjectSettings.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetSettingPath(ProjectSettingTypes type)
            => $"ProjectSettings/{type}.asset";

        public static Dictionary<string, SerializedObject> settingAssetDict = new();
        /// <summary>
        /// Get SerializedObject reference to ProjectSettings file
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SerializedObject GetAsset(ProjectSettingTypes type)
        {
            var path = GetSettingPath(type);
            if (!settingAssetDict.TryGetValue(path, out var so) || (so != null && !so.targetObject))
            {
                settingAssetDict[path] = so = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>(path));
            }
            return so;
        }
        /// <summary>
        /// Get SerializedObject reference to ProjectSettings file by type T, like PlayerSettings, TagManager, GraphicsSettings etc.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SerializedObject GetAsset<T>() where T : Object
        {
            var path = $"ProjectSettings/{typeof(T).Name}.asset";
            if (!settingAssetDict.TryGetValue(path, out var so) || (so != null && !so.targetObject))
            {
                var setting = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault();
                settingAssetDict[path] = so = new SerializedObject(setting);
            }

            return so;
        }

    }
}
#endif