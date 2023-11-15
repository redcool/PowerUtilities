using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class StructEx
    {

        public static string GetEnumName<T>(this T t) where T : struct, IConvertible
        {
            return Enum.GetName(typeof(T), t);
        }
    }
}
