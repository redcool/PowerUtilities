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


            if (inst.isOverrideUberPostShader && inst.urpUberShader)
            {
                var postData = PostProcessDataEx.GetDefaultPostProcessData();
                if (postData)
                    postData.shaders.uberPostPS = inst.urpUberShader;
            }

            //k Setup MainCamera
            var cam = Camera.main;
            if (!cam)
                return;

            if (inst.isOverrideMainCameraAntiAlise)
            {
            var addData = cam.GetUniversalAdditionalCameraData();
                if (addData) 
                    addData.antialiasing = inst.aaMode;
            }

            if (inst.isOverrideMainCameraClearColor)
            {
                cam.clearFlags = inst.clearFlags;
                cam.backgroundColor = inst.clearColor;
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
        [HelpBox]
        public string helpBox = "Unity Recoder Cpature, 1 set TargetCamera, FileFormat : PNG";

        [Header("URP FrameBuffer")]
        [Tooltip("Graphics.preserverCameraTargetAlpha ")]
        public bool preserverCameraTargetAlpha;

        [Header("Recorder")]
        [Tooltip("open shader keyword TRANSPARENCY_ON")]
        public bool isTransparencyKeywordOn = true;

        [Header("PostProcess ")]
        [Tooltip("override urp's UberPost.shader")]
        public bool isOverrideUberPostShader;
        [LoadAsset("UberPostEx.shader")]
        public Shader urpUberShader;

        [Header("Camera's AA")]
        [Tooltip("Set MainCamera's Anti-Aliasing to no")]
        public bool isOverrideMainCameraAntiAlise;
        public AntialiasingMode aaMode;

        [Header("Camera's ClearColor")]
        [Tooltip("Set MainCamera's ClearColor")]
        public bool isOverrideMainCameraClearColor;
        public CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
        public Color clearColor = Color.clear;
    }
}
#endif