using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities.RenderFeatures
{
    
    public class SRPRenderFeatureControl : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public SRPFeatureList featureList;
        }

        public Settings settings;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings == null || settings.featureList == null || settings.featureList.settingList.Count == 0)
                return;

            foreach (var feature in settings.featureList.settingList)
            {
                if (feature == null)
                    continue;

                var pass = feature.GetPass();
                if (pass == null || !feature.enabled)
                    continue;

                pass.renderPassEvent = feature.renderPassEvent;
                renderer.EnqueuePass(pass);
            }
            
        }

        public override void Create()
        {
        }
    }
}
