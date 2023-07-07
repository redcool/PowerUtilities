using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// cache object tools
    /// call Get() will get cached object or find object then cache and return .
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CacheTool<K,T> where K : class where T : class
    {
        Dictionary<K, T> dict = new Dictionary<K, T>();
        public T Get(K key, Func<T> onNotExists, bool forceRefresh = false)
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
        static CacheTool<string,Material> matCacheTool = new CacheTool<string,Material>();
        public static Material GetMaterial(string shaderPath,bool forceRefresh=false)=> matCacheTool.Get(shaderPath, () => new Material(Shader.Find(shaderPath)), forceRefresh);
        public static void Clear() => matCacheTool.Clear();
    }

    public static class TextureCacheTool
    {
        static CacheTool<string,Texture> cacheTool = new CacheTool<string,Texture>();
        public static Texture GetTexture(string texturePath, Func<Texture> onFindTexture, bool forceRefresh = false) => cacheTool.Get(texturePath, onFindTexture, forceRefresh);
        public static void Clear() => cacheTool.Clear();
    }

    public static class ObjectCacheTool
    {
        static CacheTool<Type, Object> cacheTool = new CacheTool<Type, Object>();
        public static Object GetObject(Type t, Func<Object> onNotExists) => cacheTool.Get(t, onNotExists);
    }
}
