using System;
using System.Collections.Generic;

namespace PowerUtilities
{
    public static class EnumEx
    {
        /// <summary>
        ///  enumType => names
        /// </summary>
        public readonly static Dictionary<Type, string[]> enumNamesDict = new();

        /// <summary>
        /// enumType => values
        /// </summary>
        public readonly static Dictionary<Type, Array> enumValuesDict = new();

        /// <summary>
        /// Get enum names with cache
        /// </summary>
        /// <param name="tEnum"></param>
        /// <returns></returns>
        public static string[] GetNames<TEnum>() where TEnum : Enum
            => GetNames(typeof(TEnum));

        /// <summary>
        /// Get enum names with cache
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string[] GetNames(Type enumType)
            => DictionaryTools.Get(enumNamesDict, enumType, (enumType) => Enum.GetNames(enumType));

        /// <summary>
        /// Get enum values with cache
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        public static TEnum[] GetValues<TEnum>() where TEnum : Enum
            => (TEnum[])DictionaryTools.Get(enumValuesDict, typeof(TEnum), (enumType) => Enum.GetValues(enumType));
        

        /// <summary>
        /// Iterate enum items
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="enumType"></param>
        /// <param name="action"></param>
        public static void ForEach<TEnum>(Action<string, TEnum> action) where TEnum : Enum
        {
            if (action == null)
                return;

            var enumType = typeof(TEnum);

            var names = GetNames(enumType);
            var values = GetValues<TEnum>();
            for (int i = 0; i < names.Length; i++)
            {
                action(names[i], values[i]);
            }
        }

    }
}
