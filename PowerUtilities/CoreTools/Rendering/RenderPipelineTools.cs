namespace PowerUtilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    public static class RenderPipelineTools
    {
        public static bool IsUniversalPipeline() =>  GraphicsSettings.currentRenderPipeline.GetType().Name.IsMatch("UniversalRenderPipelineAsset", StringEx.NameMatchMode.Contains);
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
    }
}