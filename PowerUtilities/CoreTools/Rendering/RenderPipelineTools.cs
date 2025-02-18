namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public static class RenderPipelineTools
    {
        /// <summary>
        /// last additionalCameraData's Renderer
        /// </summary>
        public static ScriptableRenderer lastCamRenderer;

        public static bool IsUniversalPipeline() => GraphicsSettings.currentRenderPipeline.GetType().Name.IsMatch("UniversalRenderPipelineAsset", StringEx.NameMatchMode.Contains);
        public static bool IsHDRenderPipeline() => GraphicsSettings.currentRenderPipeline.GetType().Name.IsMatch("HDRenderPipelineAsset", StringEx.NameMatchMode.Contains);

        /// <summary>
        /// will call
        /// 1 UniversalRenderPipeline.asset changed
        /// 2 Camera's renderer changed 
        /// </summary>
        public static event Action OnCameraRendererChanged;

        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        static void Init()
        {
            RenderPipelineManager.beginContextRendering -= RenderPipelineManager_beginContextRendering;
            RenderPipelineManager.beginContextRendering += RenderPipelineManager_beginContextRendering;

            RenderPipelineManager.activeRenderPipelineAssetChanged -= RenderPipelineManager_activeRenderPipelineAssetChanged;
            RenderPipelineManager.activeRenderPipelineAssetChanged += RenderPipelineManager_activeRenderPipelineAssetChanged;

            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;
            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        }

        private static void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            //var asset = UniversalRenderPipeline.asset;
            var data = cam.GetUniversalAdditionalCameraData();

            var curRenderer = data.scriptableRenderer;
            if(CompareTools.CompareAndSet(ref lastCamRenderer,ref curRenderer))
            {
                OnCameraRendererChanged?.Invoke();
            }
        }

        private static void RenderPipelineManager_activeRenderPipelineAssetChanged(RenderPipelineAsset lastAsset, RenderPipelineAsset curAsset)
        {
            // camera renderer instance changed
            OnCameraRendererChanged?.Invoke();
        }

        private static void RenderPipelineManager_beginContextRendering(ScriptableRenderContext context, List<Camera> list)
        {
            CameraList = list;
        }

        /// <summary>
        /// Camera list for urp rendering
        /// </summary>
        public static List<Camera> CameraList { get; private set; }

        /// <summary>
        /// Get next in camera list, when nextId>=cameraList count return 0
        /// </summary>
        /// <param name="curCamera"></param>
        /// <param name="nextCamId"></param>
        /// <returns></returns>
        public static Camera GetNextCamera(Camera curCamera, out int nextCamId)
        {
            nextCamId = CameraList.IndexOf(curCamera) + 1;
            if (nextCamId >= CameraList.Count)
                nextCamId = 0;
            var nextCam = CameraList[nextCamId];
            return nextCam;
        }
    }
}