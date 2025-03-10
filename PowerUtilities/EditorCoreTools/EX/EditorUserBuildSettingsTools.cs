#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    public static class EditorUserBuildSettingsTools
    {

        public static RuntimePlatform BuildTargetToPlatform(BuildTarget target) => target switch
        {
            BuildTarget.StandaloneOSX => RuntimePlatform.OSXPlayer,
            BuildTarget.StandaloneWindows => RuntimePlatform.WindowsPlayer,
            BuildTarget.StandaloneWindows64 => RuntimePlatform.WindowsPlayer,
            BuildTarget.StandaloneLinux64 => RuntimePlatform.LinuxPlayer,
            BuildTarget.Android => RuntimePlatform.Android,
            BuildTarget.iOS => RuntimePlatform.IPhonePlayer,
            BuildTarget.PS5 => RuntimePlatform.PS5,
            BuildTarget.PS4 => RuntimePlatform.PS4,
            BuildTarget.XboxOne => RuntimePlatform.XboxOne,
            BuildTarget.GameCoreXboxOne => RuntimePlatform.GameCoreXboxOne,
            BuildTarget.Stadia => RuntimePlatform.Stadia,
            BuildTarget.WebGL => RuntimePlatform.WebGLPlayer,
            BuildTarget.WSAPlayer => RuntimePlatform.WSAPlayerX64,
            BuildTarget.Switch => RuntimePlatform.Switch,
            BuildTarget.tvOS => RuntimePlatform.tvOS,
            BuildTarget.VisionOS => RuntimePlatform.VisionOS,
            //BuildTarget.EmbeddedLinux => RuntimePlatform.EmbeddedLinuxX86,
            _ => RuntimePlatform.WindowsPlayer
        };
    }
}
#endif