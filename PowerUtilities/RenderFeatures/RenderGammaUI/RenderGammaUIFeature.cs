namespace PowerUtilities.Features
{
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
    using UnityEditor;
    using System;
#endif
#if UNITY_EDITOR
    [CustomEditor(typeof(RenderGammaUIFeature))]
    public class RenderGammaUIFeatureEditor : PowerEditor<RenderGammaUIFeature>
    {
        Editor settingSOEditor;
        public override void DrawInspectorUI(RenderGammaUIFeature inst)
        {
            //========================================  gammaUISetting header
            EditorGUILayout.BeginHorizontal();
            //1 exist
            EditorGUILayout.PrefixLabel("GammaUISO:");
            inst.settingSO = (GammaUISettingSO)EditorGUILayout.ObjectField(inst.settingSO, typeof(GammaUISettingSO), false);
            //2 create new
            if(GUILayout.Button("Create New"))
            {
                var so = CreateInstance<GammaUISettingSO>();
                var urpAsset = UniversalRenderPipeline.asset;
                var nextId = Resources.FindObjectsOfTypeAll<GammaUISettingSO>().Length;

                var settingsFolder = AssetDatabaseTools.CreateFolder("Assets/PowerUtilities/", "GammaUISettings",true);
                var soPath = $"{settingsFolder}/_GammaUISO_{nextId}.asset";
                AssetDatabase.CreateAsset(so, soPath);
                AssetDatabaseTools.SaveRefresh();

                //
                var newAsset = AssetDatabase.LoadAssetAtPath<GammaUISettingSO>(soPath);
                EditorGUIUtility.PingObject(newAsset);

                inst.settingSO = newAsset;
            }
            EditorGUILayout.EndHorizontal();

            //========================================  splitter line 
            var rect = EditorGUILayout.GetControlRect(false,2);
            EditorGUITools.DrawColorLine(rect);

            //========================================  draw gammaUISetting 
            if (inst.settingSO != null)
            {
                EditorTools.CreateEditor(inst.settingSO, ref settingSOEditor);
                settingSOEditor.DrawDefaultInspector();
            }
            else
            {
                EditorGUILayout.HelpBox("No Details", MessageType.Info);
            }
        }
    }
#endif

    [Tooltip("Render scane in linear, Render UI in gamma space")]
    public class RenderGammaUIFeature : ScriptableRendererFeature
    {

        public GammaUISettingSO settingSO;

        RenderUIPass uiPass;

        /// <inheritdoc/>
        public override void Create()
        {

        }

        public static bool IsUICamera(ref CameraData cameraData, string cameraTag)
        {
            var isUICamera = false;

            if (string.IsNullOrEmpty(cameraTag))
            {
                isUICamera = QualitySettings.activeColorSpace == ColorSpace.Linear &&
                    cameraData.renderType == CameraRenderType.Overlay &&
                    (cameraData.camera.cullingMask & LayerMask.GetMask("UI")) >= 1
                    ;
            }
            else
            {
                isUICamera = cameraData.camera.CompareTag(cameraTag);
            }

            return isUICamera;
        }

        private static void SetupUICamera(ref CameraData cameraData)
        {
            cameraData.clearDepth = false; // clear depth afterwards
            cameraData.requiresDepthTexture = false;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settingSO == null)
                return;

            settingSO.logs = "";
            if (!settingSO.blitMat)
            {
                settingSO.logs = "settings.blitMat not exists";
                return;
            }

            ref var cameraData = ref renderingData.cameraData;
            var isSceneCamera = cameraData.isSceneViewCamera;
            var isUICamera = IsUICamera(ref cameraData, settingSO.cameraTag);

            if (!isUICamera && !isSceneCamera)
            {
                settingSO.logs = "UICamera not found";
                return;
            }

            if (isUICamera)
            {
                SetupUICamera(ref cameraData);
            }

            // ui rendering checks
            if (settingSO.outputTarget == OutputTarget.CameraTarget)
            {
                if ((cameraData.camera.cullingMask & settingSO.filterInfo.layers) == 0)
                {
                    settingSO.logs = "UICamera.cullingMask != settings.layerMask";
                    return;
                }
            }

            if (uiPass == null)
                uiPass = new RenderUIPass();

            uiPass.renderPassEvent = settingSO.passEvent + settingSO.passEventOffset;
            uiPass.settings = settingSO;

            renderer.EnqueuePass(uiPass);
        }

    }


}