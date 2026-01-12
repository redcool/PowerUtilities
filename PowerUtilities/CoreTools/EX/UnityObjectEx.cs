using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    /// <summary>
    /// Unity.Object extensions
    /// </summary>
    public static class UnityObjectEx
    {
        static Dictionary<Object, string> objNameDict = new();

        /// <summary>
        /// Get Unity Object's name from objNameDict
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetName(this Object obj)
        {
            return DictionaryTools.Get(objNameDict, obj, obj => obj?.name);
        }
    }
}
