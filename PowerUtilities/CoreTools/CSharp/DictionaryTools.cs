using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class DictionaryTools
    {
        /// <summary>
        /// Find value with check key
        /// </summary>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="dict"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        public static bool TryFindByKey<TK,TV>(this Dictionary<TK, TV> dict, Func<TK,bool> isValid,out TV result)
        {
            result = default;
            foreach (var k in dict.Keys)
            {
                if (isValid(k))
                {
                    result = dict[k];
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get item from dict, if not exists call onNotExists create one
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="onNotExists">call when key not exists or value is null</param>
        /// <returns></returns>

        public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> onNotExists)
        {
            var isExists = dict.TryGetValue(key, out TValue value);
            if ((onNotExists != null) && (!isExists || value == null))
                value = dict[key] = onNotExists(key);
            return value;
        }


    }
}
