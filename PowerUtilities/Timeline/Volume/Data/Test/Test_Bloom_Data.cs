
namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    [Serializable]
    public struct Test_Bloom_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float threshold;
public float intensity;
public float scatter;
public float clamp;
public Color tint;
public bool highQualityFiltering;
public BloomDownscaleMode downscale;
public int maxIterations;
public Texture dirtTexture;
public float dirtIntensity;
        

        public Type ComponentType => typeof(Bloom);

        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (Bloom)vc;
            settings.threshold.overrideState = settings.threshold.value != default;
            settings.threshold.value = threshold;
            settings.intensity.overrideState = settings.intensity.value != default;
            settings.intensity.value = intensity;
            settings.scatter.overrideState = settings.scatter.value != default;
            settings.scatter.value = scatter;
            settings.clamp.overrideState = settings.clamp.value != default;
            settings.clamp.value = clamp;
            settings.tint.overrideState = settings.tint.value != default;
            settings.tint.value = tint;
            settings.highQualityFiltering.overrideState = settings.highQualityFiltering.value != default;
            settings.highQualityFiltering.value = highQualityFiltering;
            settings.downscale.overrideState = settings.downscale.value != default;
            settings.downscale.value = downscale;
            settings.maxIterations.overrideState = settings.maxIterations.value != default;
            settings.maxIterations.value = maxIterations;
            settings.dirtTexture.overrideState = settings.dirtTexture.value != default;
            settings.dirtTexture.value = dirtTexture;
            settings.dirtIntensity.overrideState = settings.dirtIntensity.value != default;
            settings.dirtIntensity.value = dirtIntensity;
        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (Bloom)vc;
            threshold = settings.threshold.value;
            intensity = settings.intensity.value;
            scatter = settings.scatter.value;
            clamp = settings.clamp.value;
            tint = settings.tint.value;
            highQualityFiltering = settings.highQualityFiltering.value;
            downscale = settings.downscale.value;
            maxIterations = settings.maxIterations.value;
            dirtTexture = settings.dirtTexture.value;
            dirtIntensity = settings.dirtIntensity.value;
        }
    }
}
