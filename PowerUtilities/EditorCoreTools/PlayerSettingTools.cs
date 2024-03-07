#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace PowerUtilities
{
    public static class PlayerSettingTools
    {
        /// <summary>
        /// get defined macro list
        /// </summary>
        /// <returns></returns>
        public static List<string> GetDefineMacroList()
        {
#if UNITY_2021_1_OR_NEWER
            var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var defineStr = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
#else
            var defineStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif

            return defineStr.SplitBy(';').ToList();
        }

        public static void ApplyDefineMacroList(List<string> defineList)
        {
            if (defineList == null)
                return;

            var defineStr = string.Join(";", defineList);
#if UNITY_2021_1_OR_NEWER
            var buildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            PlayerSettings.SetScriptingDefineSymbols(buildTarget, defineStr);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineStr);
#endif
        }

        public static void AddMacroDefines(params string[] macros)
        {
            var defineList = GetDefineMacroList();

            foreach (var macro in macros)
            {
                if (!defineList.Contains(macro))
                {
                    defineList.Add(macro);
                }
            }
            ApplyDefineMacroList(defineList);
        }

        public static void RemoveMacroDefines(params string[] macros)
        {
            var defineList = GetDefineMacroList();

            foreach (var macro in macros)
            {
                if (defineList.Contains(macro))
                {
                    defineList.Remove(macro);
                }
            }
            
            ApplyDefineMacroList(defineList);
        }
    }
}
#endif