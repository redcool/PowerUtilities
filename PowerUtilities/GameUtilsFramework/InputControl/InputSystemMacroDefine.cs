using PowerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GameUtilsFramework
{

    public static class InputSystemMacroDefine
    {
        const string ROOT_PATH = "PowerUtilities";

        public const string UNITY_INPUT_SYSTEM = nameof(UNITY_INPUT_SYSTEM);

        /// <summary>
        /// enabled script macros(inputSystem,auto run
        /// </summary>
        //[MenuItem(ROOT_PATH + "/InputSystem/SyncInputSystemMacro")]
        [InitializeOnLoadMethod]
        public static void SyncInputSystemMacro()
        {
            if (PlayerSettingTools.IsInputSystemEnabled())
                PlayerSettingTools.AddMacroDefines(UNITY_INPUT_SYSTEM);
            else
                PlayerSettingTools.RemoveMacroDefines(UNITY_INPUT_SYSTEM);
        }

    }
}
#endif