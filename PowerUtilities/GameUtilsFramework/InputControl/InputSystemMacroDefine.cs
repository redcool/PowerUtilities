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

        public const string INPUT_SYSTEM_ENABLED = nameof(INPUT_SYSTEM_ENABLED);

        /// <summary>
        /// enabled script macros(inputSystem
        /// </summary>
        [MenuItem(ROOT_PATH + "/InputSystem/SyncInputSystemMacro")]
        public static void SyncInputSystemMacro()
        {
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().First();
            var playerSettingObject = new SerializedObject(playerSettings);

            var activeInputHandlerSP = playerSettingObject.FindProperty("activeInputHandler");
            var inputSystemEnabled = activeInputHandlerSP.intValue > 0;

            if (inputSystemEnabled)
                PlayerSettingTools.AddMacroDefines(INPUT_SYSTEM_ENABLED);
            else
                PlayerSettingTools.RemoveMacroDefines(INPUT_SYSTEM_ENABLED);

        }
    }
}
#endif