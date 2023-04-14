using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class ReflectionTools
    {
        public static IEnumerable<Type> GetTypesDerivedFrom<T>()
        {
            var tType = typeof(T);
            var types = tType.Assembly.GetTypes();
            var list = new List<Type>();
            return types.Where(t => t.BaseType != null && t.BaseType == tType);
        }
    }
}
