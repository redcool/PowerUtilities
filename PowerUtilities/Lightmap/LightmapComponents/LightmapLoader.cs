namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LightmapLoader : MonoBehaviour
    {
        public Texture2D[] lightmaps;
        public Texture2D[] shadowMasks;

        public bool isAutoLoad = true;
        // Start is called before the first frame update
        void OnEnable()
        {
            if (isAutoLoad)
                LoadLightmaps(lightmaps, shadowMasks);
        }

        public static void LoadLightmaps(Texture2D[] lightmaps, Texture2D[] shadowMasks)
        {
            var lightmapDatas = new LightmapData[lightmaps.Length];
            for (int i = 0; i < lightmaps.Length; i++)
            {
                var data = lightmapDatas[i] = new LightmapData();
                data.lightmapColor = lightmaps[i];
                data.shadowMask = shadowMasks[i];
            }
            LightmapSettings.lightmaps = lightmapDatas;
        }
    }
}