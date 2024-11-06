#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace PowerUtilities
{
    public static class TypeCacheTools
    {
        public static Type GetTypesDerivedFrom<T>(string typeName, StringEx.NameMatchMode matchMode = StringEx.NameMatchMode.Full)
        {
            var types = TypeCache.GetTypesDerivedFrom<T>();
            return types
                .Where(type => type.Name.IsMatch(typeName, matchMode))
                .FirstOrDefault();
        }
    }
}
#endif