using Jint.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace PowerUtilities
{
    public static class EnumEx
    {
        /// <summary>
        ///  enumType => names
        /// </summary>
        public readonly static Dictionary<Type, string[]> enumNamesDict = new();

        public readonly static Dictionary<Type, KeyValuePair<string,object>[]> enumNameValueDict = new();

        /// <summary>
        /// Get enum names with cache
        /// </summary>
        /// <param name="tEnum"></param>
        /// <returns></returns>
        public static string[] GetNames<TEnum>() where TEnum : Enum
            => GetNames(typeof(TEnum));

        public static string[] GetNames(Type enumType)
            => DictionaryTools.Get(enumNamesDict, enumType, (enumType) => Enum.GetNames(enumType));

        public static TValue[] GetValues<TValue>(Type enumType)
        {
            return (TValue[])Enum.GetValues(enumType);
        }

        /// <summary>
        /// Iterate enum items
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="enumType"></param>
        /// <param name="action"></param>
        public static void ForEach<TEnum>(Action<string, TEnum> action)
        {
            if (action == null)
                return;

            var enumType = typeof(TEnum);

            var names = GetNames(enumType);
            var values = GetValues<TEnum>(enumType);
            for (int i = 0; i < names.Length; i++)
            {
                action(names[i], values[i]);
            }
        }
    }
}
