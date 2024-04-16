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
                EditorGUILayout.LabelField("Blit baseCamera target", inst.isBlitActiveColorTarget.ToString());
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
        //=================== Base Options
        [EditorHeader("", "------ Base Options ------ ")]
        [Header("URP Pass Control")]
        [Tooltip("remove urp's final blit pass")]
        public bool isRemoveURPFinalBlit = true;

        [Header("Filter/Camera")]
        [Tooltip("which camera can run ,all camera is valid when empty")]
        public string cameraTag;

        [Header("Performance Options")]
        [Tooltip("Best option is close for Middle device.")]
        public bool disableFSR = true;

        [Header("Override stencil")]
        public StencilStateData stencilStateData;

        [Header("Render event")]
        public RenderPassEvent passEvent = RenderPassEvent.AfterRendering;
        public int passEventOffset = 10;

        [Header("UI options")]
        [Tooltip("rendering ui objects use overrideUIShader")]
        public bool isOverrideUIShader;
        [LoadAsset("UI-Default.shader")]
        public Shader overrideUIShader;

        //=================== blit current active
        [Header("Blit source")]
        [Tooltip("blit base camera 's target to gamma spece,rendering gamme objects need check")]
        public bool isBlitActiveColorTarget = true;

        [LoadAsset("defaultGammaUICopyColor.mat")]
        public Material blitMat;
        [Tooltip("blit BlitActiveColorTarget blend mode(will change RenderTarget's LoadAction")]
        public BlendMode blitSrcMode = BlendMode.SrcAlpha;
        public BlendMode blitDestMode = BlendMode.OneMinusSrcAlpha;

        //=================== Gamma Flow
        [EditorHeader("", "------ Gamma Flow ------ ")]
        [Header("Fullsize Texture")]
        [Tooltip("create a full size texture,as rendering objects target, otherwise use CameraColor(Depth)Attachment,FSR need this")]
        public bool createFullsizeGammaTex;

        [Tooltip("Need use stencil buffer?")]
        public DepthBufferBits depthBufferBits = DepthBufferBits._24;

        [Header("Blit destination")]
        [Tooltip("blit to CameraTarget,URP CurrentActive,or no, when rendering done")]
        public OutputTarget outputTarget = OutputTarget.CameraTarget;

        [Header("Final blit blend mode")]
        public BlendMode finalBlitSrcMode = BlendMode.One;
        public BlendMode finalBlitDestMode = BlendMode.Zero;

        //=================== Linear Flow
        [EditorHeader("","------ Linear Flow ------ ")]
        [Tooltip("Linear flow,No blit,no gamma texture,draw objects,output to camera target")]
        public bool isWriteToCameraTargetDirectly;
        public bool isClearCameraTargetDepth;

        //=================== Draw
        [EditorHeader("", "------ Draw Objects ------ ")]
        [Header("Filter/DrawObjects")]
        [Tooltip("main render object's layer")]
        public FilteringSettingsInfo filterInfo = new FilteringSettingsInfo
        {
            layers = 32,
            renderQueueRangeInfo = new RangeInfo(2501, 5000)
        };

        [Tooltip("render objects use layers, one by one")]
        public List<FilteringSettingsInfo> filterInfoList = new List<FilteringSettingsInfo>();

        //=================== Log
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