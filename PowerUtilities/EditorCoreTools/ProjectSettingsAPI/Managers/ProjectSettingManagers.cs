#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
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

        public static string GetSettingPath(ProjectSettingTypes type) 
            => $"ProjectSettings/{Enum.GetName(typeof(ProjectSettingTypes),type)}.asset";

        static CacheTool<string, SerializedObject> settingAssetCache = new CacheTool<string, SerializedObject>();

        public static SerializedObject GetAsset(ProjectSettingTypes type)
        {
            var path = GetSettingPath(type);
            return settingAssetCache.Get(path, () => new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>(path)));
        }
    }
}
#endif