#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

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
            
            var cam = Camera.main;

            if (inst.isOverrideUberPostShader && inst.urpUberShader && cam)
            {
                    var postData = PostProcessDataEx.GetDefaultPostProcessData();
                    postData.shaders.uberPostPS = inst.urpUberShader;
            }

            if (inst.isOverrideMainCameraAntiAlise && cam)
            {
                cam.GetUniversalAdditionalCameraData().antialiasing = AntialiasingMode.None;
            }
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

        [Tooltip("override urp's UberPost.shader")]
        public bool isOverrideUberPostShader;
        [LoadAsset("UberPostEx.shader")]
        public Shader urpUberShader;

        [Tooltip("Set MainCamera's Anti-Aliasing to no")]
        public bool isOverrideMainCameraAntiAlise;
        public AntialiasingMode aaMode;
    }
}
#endif