#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// handle ScriptableRenderer(UniversalRenderer) passes by reflections
    /// </summary>
    public static partial class ScriptableRendererEx
    {
        /// <summary>
        /// (ContextContainer) m_frameData is private field in ScriptableRenderer
        /// </summary>
        public static ContextContainer contextContainer;
        /// <summary>
        /// (RenderGraph) s_RenderGraph is internal static field in UniversalRenderPipeline
        /// </summary>
        public static RenderGraph renderGraph;

        /// <summary>
        /// Get current m_frameData,(ScriptableRenderer private field)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ContextContainer GetCurrentPassContextContainer(this ScriptableRenderer r) => contextContainer;

        /// <summary>
        /// Get current renderGraph (UniversalRenderPipeline static field)
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static RenderGraph GetCurrentPassRenderGraph(this ScriptableRenderer r) => renderGraph;


        public static RTHandle CameraColorTargetHandle(this ScriptableRenderer r)
        {
            return contextContainer.Get<UniversalResourceData>().activeColorTexture;
        }

        public static RTHandle CameraDepthTargetHandle(this ScriptableRenderer r)
        {
            return contextContainer.Get<UniversalResourceData>().activeDepthTexture;
        }

    }
}
#endif