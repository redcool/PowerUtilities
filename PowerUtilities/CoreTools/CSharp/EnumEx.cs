using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class EnumEx
    {

        public static TEnum Parse<TEnum>(string value) where TEnum : struct
        {
            return Parse<TEnum>(value, false);
        }
        public static TEnum Parse<TEnum>(string value, bool ignoreCase) where TEnum : struct
        {
            if (Enum.TryParse<TEnum>(value, ignoreCase, out var enumValue))
                return enumValue;

            throw new ArgumentException($"{value} not found");
        }
    }
}
