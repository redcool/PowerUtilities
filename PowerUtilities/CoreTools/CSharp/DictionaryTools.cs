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
        /// Get item from dict, if not exists call onNotExists create one
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="onNotExists">use a cached func, otherwise cause gc</param>
        /// <returns></returns>
        
        public static TValue Get<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key,Func<TKey,TValue> onNotExists)
        {
            var isExists = dict.TryGetValue(key, out TValue value);
            if (!isExists || value == null)
                value = dict[key] = onNotExists(key);
            return value;
        }
    }
}
