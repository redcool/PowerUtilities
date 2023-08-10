using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PowerUtilities
{
    /// <summary>
    /// Show a Project Settings(Preference) Group
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ProjectSettingGroupAttribute : Attribute
    {
        public enum SettingsScope
        {
            User,Project
        }

        public const string POWER_UTILS = "PowerUtils";
        /// <summary>
        /// setting's path,like "Project/XXXSettings"
        /// </summary>
        public string settingPath;

        /// <summary>
        /// Project Setting or Preferences
        /// </summary>
        public SettingsScope settingsScope = SettingsScope.User;

        /// <summary>
        /// setting's label, like XXXSettings,shown on left
        /// </summary>
        public string label = "";
        public ProjectSettingGroupAttribute(string settingPath)
        {
            this.settingPath = settingPath;
        }
    }
}
