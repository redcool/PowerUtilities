#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{
    [CustomEditor(typeof(UnityRecorderSetting))]
    public class UnityRecorderSOEditor : PowerEditor<UnityRecorderSetting>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(UnityRecorderSetting inst)
        {
            EditorGUILayout.HelpBox("Recoder Cpature, need set TargetCamera, FileFormat : PNG", MessageType.Info);
            // 
            var so = ProjectSettingManagers.GetAsset(ProjectSettingManagers.ProjectSettingTypes.ProjectSettings);
            so.UpdateIfRequiredOrScript();
            var preserveFramebufferAlpha = so.FindProperty("preserveFramebufferAlpha");
            if (preserveFramebufferAlpha.boolValue != inst.preserverCameraTargetAlpha)
            {
                preserveFramebufferAlpha.boolValue = inst.preserverCameraTargetAlpha;
            }
            so.ApplyModifiedProperties();

            // 2 TRANSPARENCY_ON
            ShaderEx.SetKeywords(inst.isTransparencyKeywordOn, "TRANSPARENCY_ON");
        }


    }

    /// <summary>
    /// Unity Recorder extends
    /// </summary>
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS+"/Tools/UnityRecorderEx")]
    [SOAssetPath("Assets/PowerUtilities/UnityRecorderEx.asset")]
    public class UnityRecorderSetting : ScriptableObject
    {
        [Tooltip("Graphics.preserverCameraTargetAlpha ")]
        public bool preserverCameraTargetAlpha;

        [Tooltip("open shader keyword TRANSPARENCY_ON")]
        public bool isTransparencyKeywordOn = true;
    }
}
#endif