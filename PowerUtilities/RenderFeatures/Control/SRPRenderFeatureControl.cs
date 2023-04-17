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
            public List<SRPRenderFeatureSO> features = new List<SRPRenderFeatureSO>();
        }

        public Settings settings;

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            
        }

        public override void Create()
        {
            
        }
    }
}
