#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace PowerUtilities
{
    public static class EditorInit
    {
        public const string POWER_UTILS = "";

        /// <summary>
        /// editor Initial first
        /// </summary>
        [InitializeOnLoadMethod]
        public static void Init()
        {
            AddPowerUtilsSymbol();
        }
        private static void AddPowerUtilsSymbol()
        {
            PlayerSettingTools.AddMacroDefines(nameof(POWER_UTILS));
        }
    }
}
#endif