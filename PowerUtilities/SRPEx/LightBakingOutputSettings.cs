namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System.IO;
    using System.Reflection;


    public class LightBakingOutputSettingsl : MonoBehaviour
    {
        [EditorButton(onClickCall = "Start")]
        public bool isTest;

        [Tooltip("SetupLightBakingOutput when Start")]
        public bool isSetOnStart = true;

        public MixedLightingMode mixedLightingMode = MixedLightingMode.IndirectOnly;
        public LightmapBakeType lightmapBakeType = LightmapBakeType.Mixed;
        public int probeOcclusionLightIndex, occlusionMaskChannel;

        // Start is called before the first frame update
        void Start()
        {
            if(isSetOnStart)
                SetupLightBakingOutput();
        }

        public void SetupLightBakingOutput()
        {
            var l = GetComponent<Light>();
            if (!l)
                return;

            var output = l.bakingOutput;
            output.mixedLightingMode = mixedLightingMode;
            output.lightmapBakeType = lightmapBakeType;
            output.probeOcclusionLightIndex = probeOcclusionLightIndex;
            output.occlusionMaskChannel = occlusionMaskChannel;

            l.bakingOutput = output;
        }

    }
}