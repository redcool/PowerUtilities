using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// cache object tools
    /// call Get() will get cached object or find object then cache and return .
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectCacheTool<T> where T : UnityEngine.Object
    {
        Dictionary<string, T> dict = new Dictionary<string, T>();
        public T Get(string key, Func<T> onNotExists,bool forceRefresh=false)
        {
            if (dict.TryGetValue(key, out T obj))
            {
                //Debug.Log("obj:"+ (obj == default));
                if (onNotExists != null && obj == null)
                {
                    return dict[key] = onNotExists();
                }
                return obj;
            }
            return onNotExists != null ? dict[key] = onNotExists() : default;
        }

        public void Clear()
        {
            dict.Clear();
        }
    }

    public static class MaterialCacheTool
    {
        static ObjectCacheTool<Material> matCacheTool = new ObjectCacheTool<Material>();
        public static Material GetMaterial(string shaderPath,bool forceRefresh=false)=> matCacheTool.Get(shaderPath, () => new Material(Shader.Find(shaderPath)), forceRefresh);
        public static void Clear() => matCacheTool.Clear();
    }

    public static class TextureCacheTool
    {
        static ObjectCacheTool<Texture> cacheTool = new ObjectCacheTool<Texture>();
        public static Texture GetTexture(string texturePath, Func<Texture> onFindTexture, bool forceRefresh = false) => cacheTool.Get(texturePath, onFindTexture, forceRefresh);
        public static void Clear() => cacheTool.Clear();
    }
}
