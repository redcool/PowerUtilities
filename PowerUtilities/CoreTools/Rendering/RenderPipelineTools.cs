namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

    public static class RenderPipelineTools
    {
        public static bool IsUniversalPipeline() => GraphicsSettings.currentRenderPipeline.GetType().Name == "UniversalRenderPipelineAsset";
        public static bool IsHDRenderPipeline() => GraphicsSettings.currentRenderPipeline.GetType().Name == "HDRenderPipelineAsset";
    }
}