namespace PowerUtilities.Features
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
    using UnityEditor;
#endif

#if UNITY_EDITOR
    [CustomEditor(typeof(GammaUISettingSO))]
    public class GammaUISettingSOEditor : PowerEditor<GammaUISettingSO>
    {

        public override void DrawInspectorUI(GammaUISettingSO inst)
        {
            GUILayout.BeginVertical("Box");
            DrawDefaultInspector();
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");

            var lastColor = GUI.contentColor;
            GUI.contentColor = Color.green;
            inst.isShowStates = EditorGUILayout.Foldout(inst.isShowStates, "--- Current States", true);
            GUI.contentColor = lastColor;

            if (inst.isShowStates)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Flow : ", inst.isWriteToCameraTargetDirectly ? "Linear" : "Gamma");
                EditorGUILayout.LabelField("Blit baseCamera target", inst.isBlitBaseCameraTarget.ToString());
                EditorGUILayout.LabelField($"Final Blit to ", inst.outputTarget.ToString());
                EditorGUILayout.LabelField($"Final blit blend", $" src : {inst.finalBlitSrcMode}, dst : {inst.finalBlitDestMode}");

                EditorGUI.indentLevel--;
            }
            GUILayout.EndVertical();

        }
    }

    partial class GammaUISettingSO
    {
        [HideInInspector]
        public bool isShowStates;
    }
#endif

    public enum OutputTarget
    {
        /// <summary>
        /// device's CameraTarget
        /// </summary>
        CameraTarget,
        UrpColorTarget,
        None
    }

    [Serializable]
    public partial class GammaUISettingSO : ScriptableObject
    {
        [EditorHeader("", "------ Base Options ------ ")]
        [Header("URP Pass Control")]
        public bool isRemoveURPFinalBlit = true;

        [Header("Filter/Camera")]
        [Tooltip("which camera can run ,all camera is valid when empty")]
        public string cameraTag;

        [Header("Blit options")]
        [Tooltip("blit base camera 's target to gamma spece,rendering gamme objects need check")]
        public bool isBlitBaseCameraTarget = true;

        [LoadAsset("defaultGammaUICopyColor.mat")]
        public Material blitMat;

        [Header("Performance Options")]
        [Tooltip("Best option is close for Middle device.")]
        public bool disableFSR = true;

        [Header("Override stencil")]
        public StencilStateData stencilStateData;

        [Header("Render event")]
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
        public int passEventOffset = 10;

        [Header("UI options")]
        [Tooltip("ui objects use")]
        public bool isOverrideUIShader;
        [LoadAsset("UI-Default.shader")]
        public Shader overrideUIShader;

        [EditorHeader("", "------ Gamma Flow ------ ")]
        [Header("Blit destination options")]
        [Tooltip("blit to CameraTarget,URP CurrentActive,or no, when rendering done")]
        public OutputTarget outputTarget = OutputTarget.CameraTarget;

        [Header("Final blit blend mode")]
        public BlendMode finalBlitSrcMode = BlendMode.One;
        public BlendMode finalBlitDestMode = BlendMode.Zero;

        [Header("Filter/DrawObjects")]
        [Tooltip("main render object's layer")]
        public FilteringSettingsInfo filterInfo = new FilteringSettingsInfo
        {
            layers = 32,
            renderQueueRangeInfo = new RangeInfo(2501, 5000)
        };

        [Tooltip("render objects use layers, one by one")]
        public List<FilteringSettingsInfo> filterInfoList = new List<FilteringSettingsInfo>();

        [Header("Fullsize Texture")]
        [Tooltip("create a full size texture,as rendering objects target, otherwise use CameraColor(Depth)Attachment,FSR need this")]
        public bool createFullsizeGammaTex;

        [Tooltip("Need use stencil buffer?")]
        public DepthBufferBits depthBufferBits = DepthBufferBits._24;

        [EditorHeader("","------ Linear Flow ------ ")]
        [Tooltip("simple rendering flow,No blit,no gamma texture,draw objects,output to camera target")]
        public bool isWriteToCameraTargetDirectly;
        public bool isClearCameraTarget;


        [Header("Editor Options")]
        [Multiline]
        public string logs;


        Material uiMat;
        public Material UIMaterial
        {
            get
            {
                if (!uiMat && overrideUIShader)
                    uiMat = new Material(overrideUIShader);
                return uiMat;
            }
        }
    }

}