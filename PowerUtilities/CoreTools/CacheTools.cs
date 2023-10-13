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
    /// <typeparam name="V"></typeparam>
    public class CacheTool<K,V> where K : class where V : class
    {
        Dictionary<K, V> dict = new Dictionary<K, V>();

        /// <summary>
        /// get vallue from dict
        /// </summary>
        /// <param name="key"></param>
        /// <param name="onNotExists"></param>
        /// <param name="isValueValid"></param>
        /// <returns></returns>
        public V Get(K key, Func<V> onNotExists,Func<V,bool> isValueValid = null)
        {
            //get a default predicate
            isValueValid = isValueValid ?? IsValid;

            // try get
            if (dict.TryGetValue(key, out V obj))
            {
                // has value,but is null
                if (onNotExists != null)
                {
                    if (!isValueValid(obj))
                        return dict[key] = onNotExists();
                }
                return obj;
            }

            // no key
            return onNotExists != null ? dict[key] = onNotExists() : default;
        }

        /// <summary>
        /// Check obj is valid
        /// 1 object
        /// 2 SerializedObject
        /// 3 Editor
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsValid(V obj)
        {
            var isValid = obj != null;
#if UNITY_EDITOR
            if (isValid && obj is UnityEditor.SerializedObject so)
                isValid = isValid && so.targetObject != null;

            if (isValid && obj is UnityEditor.Editor e)
                isValid = isValid && e.target != null;
#endif
            return isValid;
        }

        public void Clear() => dict.Clear();

        public void Set(K key, V value) => dict[key] = value;

        public void Remove(K key) => dict.Remove(key);
    }

    public static class MaterialCacheTool
    {
        static CacheTool<string,Material> matCacheTool = new CacheTool<string,Material>();
        public static Material GetMaterial(string shaderPath)=> matCacheTool.Get(shaderPath, () => new Material(Shader.Find(shaderPath)));
        public static void Clear() => matCacheTool.Clear();
    }

    public static class TextureCacheTool
    {
        static CacheTool<string,Texture> cacheTool = new CacheTool<string,Texture>();
        public static Texture GetTexture(string texturePath, Func<Texture> onFindTexture) => cacheTool.Get(texturePath, onFindTexture);
        public static void Clear() => cacheTool.Clear();
    }

}
