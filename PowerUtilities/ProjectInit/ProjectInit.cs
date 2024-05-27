using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class ProjectInit
    {
        /// <summary>
        /// Initial first(
        /// </summary>
        [RuntimeInitializeOnLoadMethod]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        public static void Init()
        {
            InitRT();
        }

        public static void InitRT()
        {
            Shader.SetGlobalTexture(ShaderPropertyIds._MainLightShadowmapTexture, RenderingTools.EmptyShadowMap);
            Shader.SetGlobalTexture(ShaderPropertyIds._AdditionalLightsShadowmapTexture, RenderingTools.EmptyShadowMap);

            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_beginCameraRendering;
        }

        private static void RenderPipelineManager_beginCameraRendering(ScriptableRenderContext context, Camera cam)
        {
            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_beginCameraRendering;

            var cmd = CommandBufferEx.defaultCmd;
            cmd.Execute(ref context);

            cmd.SetRenderTarget(RenderingTools.EmptyShadowMap);
            cmd.ClearRenderTarget(true, false, Color.clear);
        }
    }
}
