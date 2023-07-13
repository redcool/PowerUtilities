using System.Linq;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    public static class UniversalRenderPipelineDataEx
    {
        public static T[] GetFeatures<T>(this UniversalRendererData data) where T : ScriptableRendererFeature
        {
            return data.rendererFeatures.FindAll(f => f.GetType() == typeof(T))
                .Select(f => (T)f)
                .ToArray();
        }

        public static T GetFeature<T>(this UniversalRendererData data) where T : ScriptableRendererFeature
        {
            return GetFeatures<T>(data).FirstOrDefault();
        }
    }
}
