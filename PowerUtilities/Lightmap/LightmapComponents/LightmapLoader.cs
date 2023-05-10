namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(LightmapLoader))]
    public class LightmapLoaderEditor : PowerEditor<LightmapLoader>
    {
        public override bool NeedDrawDefaultUI() => true;
        public override void DrawInspectorUI(LightmapLoader inst)
        {
            GUILayout.Label("Options");
            if (GUILayout.Button("Load Lightmaps"))
            {
                LightmapLoader.LoadLightmaps(inst.lightmaps, inst.shadowMasks);
            }
        }
    }
#endif

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
                data.shadowMask = i < shadowMasks.Length ? shadowMasks[i] : null;
            }
            LightmapSettings.lightmaps = lightmapDatas;
        }
    }
}