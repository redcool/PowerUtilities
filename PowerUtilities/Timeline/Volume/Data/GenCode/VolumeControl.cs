///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct Bloom_Data : IvolumeData
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
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ChannelMixer_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float redOutRedIn;
public float redOutGreenIn;
public float redOutBlueIn;
public float greenOutRedIn;
public float greenOutGreenIn;
public float greenOutBlueIn;
public float blueOutRedIn;
public float blueOutGreenIn;
public float blueOutBlueIn;

        public Type ComponentType => typeof(ChannelMixer);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (ChannelMixer)vc;
settings.redOutRedIn.overrideState = settings.redOutRedIn.value != default;
settings.redOutRedIn.value = redOutRedIn;
settings.redOutGreenIn.overrideState = settings.redOutGreenIn.value != default;
settings.redOutGreenIn.value = redOutGreenIn;
settings.redOutBlueIn.overrideState = settings.redOutBlueIn.value != default;
settings.redOutBlueIn.value = redOutBlueIn;
settings.greenOutRedIn.overrideState = settings.greenOutRedIn.value != default;
settings.greenOutRedIn.value = greenOutRedIn;
settings.greenOutGreenIn.overrideState = settings.greenOutGreenIn.value != default;
settings.greenOutGreenIn.value = greenOutGreenIn;
settings.greenOutBlueIn.overrideState = settings.greenOutBlueIn.value != default;
settings.greenOutBlueIn.value = greenOutBlueIn;
settings.blueOutRedIn.overrideState = settings.blueOutRedIn.value != default;
settings.blueOutRedIn.value = blueOutRedIn;
settings.blueOutGreenIn.overrideState = settings.blueOutGreenIn.value != default;
settings.blueOutGreenIn.value = blueOutGreenIn;
settings.blueOutBlueIn.overrideState = settings.blueOutBlueIn.value != default;
settings.blueOutBlueIn.value = blueOutBlueIn;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (ChannelMixer)vc;
redOutRedIn = settings.redOutRedIn.value;
redOutGreenIn = settings.redOutGreenIn.value;
redOutBlueIn = settings.redOutBlueIn.value;
greenOutRedIn = settings.greenOutRedIn.value;
greenOutGreenIn = settings.greenOutGreenIn.value;
greenOutBlueIn = settings.greenOutBlueIn.value;
blueOutRedIn = settings.blueOutRedIn.value;
blueOutGreenIn = settings.blueOutGreenIn.value;
blueOutBlueIn = settings.blueOutBlueIn.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ChromaticAberration_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float intensity;

        public Type ComponentType => typeof(ChromaticAberration);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (ChromaticAberration)vc;
settings.intensity.overrideState = settings.intensity.value != default;
settings.intensity.value = intensity;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (ChromaticAberration)vc;
intensity = settings.intensity.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ColorAdjustments_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float postExposure;
public float contrast;
public Color colorFilter;
public float hueShift;
public float saturation;

        public Type ComponentType => typeof(ColorAdjustments);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (ColorAdjustments)vc;
settings.postExposure.overrideState = settings.postExposure.value != default;
settings.postExposure.value = postExposure;
settings.contrast.overrideState = settings.contrast.value != default;
settings.contrast.value = contrast;
settings.colorFilter.overrideState = settings.colorFilter.value != default;
settings.colorFilter.value = colorFilter;
settings.hueShift.overrideState = settings.hueShift.value != default;
settings.hueShift.value = hueShift;
settings.saturation.overrideState = settings.saturation.value != default;
settings.saturation.value = saturation;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (ColorAdjustments)vc;
postExposure = settings.postExposure.value;
contrast = settings.contrast.value;
colorFilter = settings.colorFilter.value;
hueShift = settings.hueShift.value;
saturation = settings.saturation.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ColorCurves_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public TextureCurve master;
public TextureCurve red;
public TextureCurve green;
public TextureCurve blue;
public TextureCurve hueVsHue;
public TextureCurve hueVsSat;
public TextureCurve satVsSat;
public TextureCurve lumVsSat;

        public Type ComponentType => typeof(ColorCurves);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (ColorCurves)vc;
settings.master.overrideState = settings.master.value != default;
settings.master.value = master;
settings.red.overrideState = settings.red.value != default;
settings.red.value = red;
settings.green.overrideState = settings.green.value != default;
settings.green.value = green;
settings.blue.overrideState = settings.blue.value != default;
settings.blue.value = blue;
settings.hueVsHue.overrideState = settings.hueVsHue.value != default;
settings.hueVsHue.value = hueVsHue;
settings.hueVsSat.overrideState = settings.hueVsSat.value != default;
settings.hueVsSat.value = hueVsSat;
settings.satVsSat.overrideState = settings.satVsSat.value != default;
settings.satVsSat.value = satVsSat;
settings.lumVsSat.overrideState = settings.lumVsSat.value != default;
settings.lumVsSat.value = lumVsSat;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (ColorCurves)vc;
master = settings.master.value;
red = settings.red.value;
green = settings.green.value;
blue = settings.blue.value;
hueVsHue = settings.hueVsHue.value;
hueVsSat = settings.hueVsSat.value;
satVsSat = settings.satVsSat.value;
lumVsSat = settings.lumVsSat.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ColorLookup_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public Texture texture;
public float contribution;

        public Type ComponentType => typeof(ColorLookup);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (ColorLookup)vc;
settings.texture.overrideState = settings.texture.value != default;
settings.texture.value = texture;
settings.contribution.overrideState = settings.contribution.value != default;
settings.contribution.value = contribution;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (ColorLookup)vc;
texture = settings.texture.value;
contribution = settings.contribution.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct DepthOfField_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public DepthOfFieldMode mode;
public float gaussianStart;
public float gaussianEnd;
public float gaussianMaxRadius;
public bool highQualitySampling;
public float focusDistance;
public float aperture;
public float focalLength;
public int bladeCount;
public float bladeCurvature;
public float bladeRotation;

        public Type ComponentType => typeof(DepthOfField);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (DepthOfField)vc;
settings.mode.overrideState = settings.mode.value != default;
settings.mode.value = mode;
settings.gaussianStart.overrideState = settings.gaussianStart.value != default;
settings.gaussianStart.value = gaussianStart;
settings.gaussianEnd.overrideState = settings.gaussianEnd.value != default;
settings.gaussianEnd.value = gaussianEnd;
settings.gaussianMaxRadius.overrideState = settings.gaussianMaxRadius.value != default;
settings.gaussianMaxRadius.value = gaussianMaxRadius;
settings.highQualitySampling.overrideState = settings.highQualitySampling.value != default;
settings.highQualitySampling.value = highQualitySampling;
settings.focusDistance.overrideState = settings.focusDistance.value != default;
settings.focusDistance.value = focusDistance;
settings.aperture.overrideState = settings.aperture.value != default;
settings.aperture.value = aperture;
settings.focalLength.overrideState = settings.focalLength.value != default;
settings.focalLength.value = focalLength;
settings.bladeCount.overrideState = settings.bladeCount.value != default;
settings.bladeCount.value = bladeCount;
settings.bladeCurvature.overrideState = settings.bladeCurvature.value != default;
settings.bladeCurvature.value = bladeCurvature;
settings.bladeRotation.overrideState = settings.bladeRotation.value != default;
settings.bladeRotation.value = bladeRotation;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (DepthOfField)vc;
mode = settings.mode.value;
gaussianStart = settings.gaussianStart.value;
gaussianEnd = settings.gaussianEnd.value;
gaussianMaxRadius = settings.gaussianMaxRadius.value;
highQualitySampling = settings.highQualitySampling.value;
focusDistance = settings.focusDistance.value;
aperture = settings.aperture.value;
focalLength = settings.focalLength.value;
bladeCount = settings.bladeCount.value;
bladeCurvature = settings.bladeCurvature.value;
bladeRotation = settings.bladeRotation.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct FilmGrain_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public FilmGrainLookup type;
public float intensity;
public float response;
public Texture texture;

        public Type ComponentType => typeof(FilmGrain);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (FilmGrain)vc;
settings.type.overrideState = settings.type.value != default;
settings.type.value = type;
settings.intensity.overrideState = settings.intensity.value != default;
settings.intensity.value = intensity;
settings.response.overrideState = settings.response.value != default;
settings.response.value = response;
settings.texture.overrideState = settings.texture.value != default;
settings.texture.value = texture;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (FilmGrain)vc;
type = settings.type.value;
intensity = settings.intensity.value;
response = settings.response.value;
texture = settings.texture.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct LensDistortion_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float intensity;
public float xMultiplier;
public float yMultiplier;
public Vector2 center;
public float scale;

        public Type ComponentType => typeof(LensDistortion);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (LensDistortion)vc;
settings.intensity.overrideState = settings.intensity.value != default;
settings.intensity.value = intensity;
settings.xMultiplier.overrideState = settings.xMultiplier.value != default;
settings.xMultiplier.value = xMultiplier;
settings.yMultiplier.overrideState = settings.yMultiplier.value != default;
settings.yMultiplier.value = yMultiplier;
settings.center.overrideState = settings.center.value != default;
settings.center.value = center;
settings.scale.overrideState = settings.scale.value != default;
settings.scale.value = scale;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (LensDistortion)vc;
intensity = settings.intensity.value;
xMultiplier = settings.xMultiplier.value;
yMultiplier = settings.yMultiplier.value;
center = settings.center.value;
scale = settings.scale.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct LiftGammaGain_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public Vector4 lift;
public Vector4 gamma;
public Vector4 gain;

        public Type ComponentType => typeof(LiftGammaGain);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (LiftGammaGain)vc;
settings.lift.overrideState = settings.lift.value != default;
settings.lift.value = lift;
settings.gamma.overrideState = settings.gamma.value != default;
settings.gamma.value = gamma;
settings.gain.overrideState = settings.gain.value != default;
settings.gain.value = gain;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (LiftGammaGain)vc;
lift = settings.lift.value;
gamma = settings.gamma.value;
gain = settings.gain.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct MotionBlur_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public MotionBlurMode mode;
public MotionBlurQuality quality;
public float intensity;
public float clamp;

        public Type ComponentType => typeof(MotionBlur);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (MotionBlur)vc;
settings.mode.overrideState = settings.mode.value != default;
settings.mode.value = mode;
settings.quality.overrideState = settings.quality.value != default;
settings.quality.value = quality;
settings.intensity.overrideState = settings.intensity.value != default;
settings.intensity.value = intensity;
settings.clamp.overrideState = settings.clamp.value != default;
settings.clamp.value = clamp;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (MotionBlur)vc;
mode = settings.mode.value;
quality = settings.quality.value;
intensity = settings.intensity.value;
clamp = settings.clamp.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct PaniniProjection_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float distance;
public float cropToFit;

        public Type ComponentType => typeof(PaniniProjection);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (PaniniProjection)vc;
settings.distance.overrideState = settings.distance.value != default;
settings.distance.value = distance;
settings.cropToFit.overrideState = settings.cropToFit.value != default;
settings.cropToFit.value = cropToFit;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (PaniniProjection)vc;
distance = settings.distance.value;
cropToFit = settings.cropToFit.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ShadowsMidtonesHighlights_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public Vector4 shadows;
public Vector4 midtones;
public Vector4 highlights;
public float shadowsStart;
public float shadowsEnd;
public float highlightsStart;
public float highlightsEnd;

        public Type ComponentType => typeof(ShadowsMidtonesHighlights);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (ShadowsMidtonesHighlights)vc;
settings.shadows.overrideState = settings.shadows.value != default;
settings.shadows.value = shadows;
settings.midtones.overrideState = settings.midtones.value != default;
settings.midtones.value = midtones;
settings.highlights.overrideState = settings.highlights.value != default;
settings.highlights.value = highlights;
settings.shadowsStart.overrideState = settings.shadowsStart.value != default;
settings.shadowsStart.value = shadowsStart;
settings.shadowsEnd.overrideState = settings.shadowsEnd.value != default;
settings.shadowsEnd.value = shadowsEnd;
settings.highlightsStart.overrideState = settings.highlightsStart.value != default;
settings.highlightsStart.value = highlightsStart;
settings.highlightsEnd.overrideState = settings.highlightsEnd.value != default;
settings.highlightsEnd.value = highlightsEnd;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (ShadowsMidtonesHighlights)vc;
shadows = settings.shadows.value;
midtones = settings.midtones.value;
highlights = settings.highlights.value;
shadowsStart = settings.shadowsStart.value;
shadowsEnd = settings.shadowsEnd.value;
highlightsStart = settings.highlightsStart.value;
highlightsEnd = settings.highlightsEnd.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct SplitToning_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public Color shadows;
public Color highlights;
public float balance;

        public Type ComponentType => typeof(SplitToning);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (SplitToning)vc;
settings.shadows.overrideState = settings.shadows.value != default;
settings.shadows.value = shadows;
settings.highlights.overrideState = settings.highlights.value != default;
settings.highlights.value = highlights;
settings.balance.overrideState = settings.balance.value != default;
settings.balance.value = balance;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (SplitToning)vc;
shadows = settings.shadows.value;
highlights = settings.highlights.value;
balance = settings.balance.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct Tonemapping_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public TonemappingMode mode;
public NeutralRangeReductionMode neutralHDRRangeReductionMode;
public HDRACESPreset acesPreset;
public float hueShiftAmount;
public bool detectPaperWhite;
public float paperWhite;
public bool detectBrightnessLimits;
public float minNits;
public float maxNits;

        public Type ComponentType => typeof(Tonemapping);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (Tonemapping)vc;
settings.mode.overrideState = settings.mode.value != default;
settings.mode.value = mode;
settings.neutralHDRRangeReductionMode.overrideState = settings.neutralHDRRangeReductionMode.value != default;
settings.neutralHDRRangeReductionMode.value = neutralHDRRangeReductionMode;
settings.acesPreset.overrideState = settings.acesPreset.value != default;
settings.acesPreset.value = acesPreset;
settings.hueShiftAmount.overrideState = settings.hueShiftAmount.value != default;
settings.hueShiftAmount.value = hueShiftAmount;
settings.detectPaperWhite.overrideState = settings.detectPaperWhite.value != default;
settings.detectPaperWhite.value = detectPaperWhite;
settings.paperWhite.overrideState = settings.paperWhite.value != default;
settings.paperWhite.value = paperWhite;
settings.detectBrightnessLimits.overrideState = settings.detectBrightnessLimits.value != default;
settings.detectBrightnessLimits.value = detectBrightnessLimits;
settings.minNits.overrideState = settings.minNits.value != default;
settings.minNits.value = minNits;
settings.maxNits.overrideState = settings.maxNits.value != default;
settings.maxNits.value = maxNits;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (Tonemapping)vc;
mode = settings.mode.value;
neutralHDRRangeReductionMode = settings.neutralHDRRangeReductionMode.value;
acesPreset = settings.acesPreset.value;
hueShiftAmount = settings.hueShiftAmount.value;
detectPaperWhite = settings.detectPaperWhite.value;
paperWhite = settings.paperWhite.value;
detectBrightnessLimits = settings.detectBrightnessLimits.value;
minNits = settings.minNits.value;
maxNits = settings.maxNits.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct Vignette_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public Color color;
public Vector2 center;
public float intensity;
public float smoothness;
public bool rounded;

        public Type ComponentType => typeof(Vignette);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (Vignette)vc;
settings.color.overrideState = settings.color.value != default;
settings.color.value = color;
settings.center.overrideState = settings.center.value != default;
settings.center.value = center;
settings.intensity.overrideState = settings.intensity.value != default;
settings.intensity.value = intensity;
settings.smoothness.overrideState = settings.smoothness.value != default;
settings.smoothness.value = smoothness;
settings.rounded.overrideState = settings.rounded.value != default;
settings.rounded.value = rounded;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (Vignette)vc;
color = settings.color.value;
center = settings.center.value;
intensity = settings.intensity.value;
smoothness = settings.smoothness.value;
rounded = settings.rounded.value;

        }
    }
}///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
///
namespace PowerUtilities
{
    using System;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct WhiteBalance_Data : IvolumeData
    {
        // variables
        public bool isEnable;
        public float temperature;
public float tint;

        public Type ComponentType => typeof(WhiteBalance);
        public void UpdateSetting(VolumeComponent vc)
        {
            var settings = (WhiteBalance)vc;
settings.temperature.overrideState = settings.temperature.value != default;
settings.temperature.value = temperature;
settings.tint.overrideState = settings.tint.value != default;
settings.tint.value = tint;

        }

        public void RecordSetting(VolumeComponent vc)
        {
            var settings = (WhiteBalance)vc;
temperature = settings.temperature.value;
tint = settings.tint.value;

        }
    }
}