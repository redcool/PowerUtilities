using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    public static class RenderFeatureManager
    {
       static Dictionary<string, ScriptableRendererFeature> featureDict = new Dictionary<string, ScriptableRendererFeature>();

        public static void Add(string name, ScriptableRendererFeature feature)
        {
            featureDict[name] = feature;
        }

        public static ScriptableRendererFeature Get<T>(string name) where T : ScriptableRendererFeature
        {
            if (featureDict.TryGetValue(name, out var feature))
                return feature;
            return null;
        }
    }
}
