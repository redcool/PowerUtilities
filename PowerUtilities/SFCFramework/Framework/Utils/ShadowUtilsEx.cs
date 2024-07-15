using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    public static class ShadowUtilsEx
    {

        public static float SoftShadowQualityToShaderProperty(Light light, bool softShadowsEnabled)
        {
            float softShadows = softShadowsEnabled ? 1.0f : 0.0f;
            if (light.TryGetComponent(out UniversalAdditionalLightData additionalLightData))
            {
                var softShadowQuality = (additionalLightData.softShadowQuality == SoftShadowQuality.UsePipelineSettings)
                    //? UniversalRenderPipeline.asset?.softShadowQuality
                    ? (SoftShadowQuality)SerializedObjectTools.GetProperty(UniversalRenderPipeline.asset, "m_SoftShadowQuality").enumValueIndex
                    : additionalLightData.softShadowQuality;
                softShadows *= Math.Max((int)softShadowQuality, (int)SoftShadowQuality.Low);
            }

            return softShadows;
        }

        /// copy from ShadowUtils
        public static void GetScaleAndBiasForLinearDistanceFade(float fadeDistance, float border, out float scale, out float bias)
        {
            // To avoid division from zero
            // This values ensure that fade within cascade will be 0 and outside 1
            if (border < 0.0001f)
            {
                float multiplier = 1000f; // To avoid blending if difference is in fractions
                scale = multiplier;
                bias = -fadeDistance * multiplier;
                return;
            }

            border = 1 - border;
            border *= border;

            // Fade with distance calculation is just a linear fade from 90% of fade distance to fade distance. 90% arbitrarily chosen but should work well enough.
            float distanceFadeNear = border * fadeDistance;
            scale = 1.0f / (fadeDistance - distanceFadeNear);
            bias = -distanceFadeNear / (fadeDistance - distanceFadeNear);
        }
    }
}
