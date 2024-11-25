
namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct Bloom_Data
    {
        // variables
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


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ChannelMixer_Data
    {
        // variables
        public float redOutRedIn;
public float redOutGreenIn;
public float redOutBlueIn;
public float greenOutRedIn;
public float greenOutGreenIn;
public float greenOutBlueIn;
public float blueOutRedIn;
public float blueOutGreenIn;
public float blueOutBlueIn;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ChromaticAberration_Data
    {
        // variables
        public float intensity;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ColorAdjustments_Data
    {
        // variables
        public float postExposure;
public float contrast;
public Color colorFilter;
public float hueShift;
public float saturation;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ColorCurves_Data
    {
        // variables
        public TextureCurve master;
public TextureCurve red;
public TextureCurve green;
public TextureCurve blue;
public TextureCurve hueVsHue;
public TextureCurve hueVsSat;
public TextureCurve satVsSat;
public TextureCurve lumVsSat;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ColorLookup_Data
    {
        // variables
        public Texture texture;
public float contribution;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct DepthOfField_Data
    {
        // variables
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


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct FilmGrain_Data
    {
        // variables
        public FilmGrainLookup type;
public float intensity;
public float response;
public Texture texture;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct LensDistortion_Data
    {
        // variables
        public float intensity;
public float xMultiplier;
public float yMultiplier;
public Vector2 center;
public float scale;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct LiftGammaGain_Data
    {
        // variables
        public Vector4 lift;
public Vector4 gamma;
public Vector4 gain;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct MotionBlur_Data
    {
        // variables
        public MotionBlurMode mode;
public MotionBlurQuality quality;
public float intensity;
public float clamp;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct PaniniProjection_Data
    {
        // variables
        public float distance;
public float cropToFit;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct ShadowsMidtonesHighlights_Data
    {
        // variables
        public Vector4 shadows;
public Vector4 midtones;
public Vector4 highlights;
public float shadowsStart;
public float shadowsEnd;
public float highlightsStart;
public float highlightsEnd;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct SplitToning_Data
    {
        // variables
        public Color shadows;
public Color highlights;
public float balance;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct Tonemapping_Data
    {
        // variables
        public TonemappingMode mode;
public NeutralRangeReductionMode neutralHDRRangeReductionMode;
public HDRACESPreset acesPreset;
public float hueShiftAmount;
public bool detectPaperWhite;
public float paperWhite;
public bool detectBrightnessLimits;
public float minNits;
public float maxNits;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct Vignette_Data
    {
        // variables
        public Color color;
public Vector2 center;
public float intensity;
public float smoothness;
public bool rounded;


    }
}

namespace PowerUtilities
{
    using System;

    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;

    [Serializable]
    public struct WhiteBalance_Data
    {
        // variables
        public float temperature;
public float tint;


    }
}
