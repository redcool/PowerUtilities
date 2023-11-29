using System.Linq;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// extends ScriptableRendererData
    /// </summary>
    public static class ScriptableRendererDataEx
    {
        public static T[] GetFeatures<T>(this ScriptableRendererData data) where T : ScriptableRendererFeature
        {
            return data.rendererFeatures.FindAll(f => f.GetType() == typeof(T))
                .Select(f => (T)f)
                .ToArray();
        }

        public static T GetFeature<T>(this ScriptableRendererData data) where T : ScriptableRendererFeature
        {
            return GetFeatures<T>(data).FirstOrDefault();
        }
    }
}
