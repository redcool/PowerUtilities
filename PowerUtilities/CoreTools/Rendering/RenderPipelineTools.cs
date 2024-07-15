namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public static class RenderPipelineTools
    {


        public static bool IsUniversalPipeline() => GraphicsSettings.currentRenderPipeline.GetType().Name.IsMatch("UniversalRenderPipelineAsset", StringEx.NameMatchMode.Contains);
        public static bool IsHDRenderPipeline() => GraphicsSettings.currentRenderPipeline.GetType().Name.IsMatch("HDRenderPipelineAsset", StringEx.NameMatchMode.Contains);

        static UniversalRenderPipelineAsset urpAsset;
        public static UniversalRenderPipelineAsset UrpAsset
        {
            get
            {
                if (urpAsset == null)
                    urpAsset = UniversalRenderPipeline.asset;
                return urpAsset;
            }
        }


        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        static void Init()
        {
            RenderPipelineManager.beginContextRendering -= RenderPipelineManager_beginContextRendering;
            RenderPipelineManager.beginContextRendering += RenderPipelineManager_beginContextRendering;
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