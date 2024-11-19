namespace PowerUtilities
{
    using System;
    using UnityEngine;

    [Serializable]
    public class BigShadowResQualitySetting
    {
        [EnumFlags(isFlags = false, type = typeof(QualitySettings), memberName = "names")]
        public int qualityLevel = 3;
        public TextureResolution res = TextureResolution.x512;
    }
}