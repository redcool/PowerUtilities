using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// handle UniversalRenderer passes by reflections
    /// </summary>
    public static class UniversalRendererPassTools
    {
        /// <summary>
        /// UniversalRenderer's private pass variables names
        /// </summary>

        public enum PassVariableNames
        {
            m_DepthPrepass,
            m_DepthNormalPrepass,
            m_PrimedDepthCopyPass,
            m_MotionVectorPass,
            m_MainLightShadowCasterPass,
            m_AdditionalLightsShadowCasterPass,
            m_GBufferPass,
            m_GBufferCopyDepthPass,
            m_DeferredPass,
            m_RenderOpaqueForwardOnlyPass,
            m_RenderOpaqueForwardPass,
            m_RenderOpaqueForwardWithRenderingLayersPass,
            m_DrawSkyboxPass,
            m_CopyDepthPass,
            m_CopyColorPass,
            m_TransparentSettingsPass,
            m_RenderTransparentForwardPass,
            m_OnRenderObjectCallbackPass,
            m_FinalBlitPass,
            m_CapturePass,

            m_XROcclusionMeshPass,
            m_XRCopyDepthPass,

            m_FinalDepthCopyPass,
            m_ProbeVolumeDebugPass,

            m_DrawOffscreenUIPass,
            m_DrawOverlayUIPass,
        }

        static Dictionary<PassVariableNames, ScriptableRenderPass> passDict = new Dictionary<PassVariableNames, ScriptableRenderPass>();

        /// <summary>
        /// get renderPass with cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="renderer"></param>
        /// <param name="passName"></param>
        /// <returns></returns>
        public static T GetRenderPass<T>(this UniversalRenderer renderer, PassVariableNames passName) where T : ScriptableRenderPass
        {
            var varName = Enum.GetName(typeof(PassVariableNames), passName);

            passDict.TryGetValue(passName, out var value);
            if (value == null)
            {
                value = passDict[passName] = typeof(UniversalRenderer).GetFieldValue<T>(renderer, varName);
            }

            return value != null ? (T)value : default;
        }
    }
}