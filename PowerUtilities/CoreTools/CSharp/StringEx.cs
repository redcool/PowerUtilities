namespace PowerUtilities
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class StringEx
    {

        static Regex kvRegex = new Regex(@"\s*=\s*");

        /// <summary>
        /// Name match rules
        /// </summary>
        public enum NameMatchMode
        {
            Full,
            StartWith,
            Contains,
            EndWith,
        }

        /// <summary>
        /// Is matchStr satisfy str ?
        /// </summary>
        /// <param str="str"></param>
        /// <param str="matchStr"></param>
        /// <param str="matchMode"></param>
        /// <returns></returns>
        public static bool IsMatch(this string str, string matchStr, NameMatchMode matchMode) => matchMode switch
        {
            NameMatchMode.Full => str == matchStr,
            NameMatchMode.Contains => str.Contains(matchStr),
            NameMatchMode.StartWith => str.StartsWith(matchStr),
            NameMatchMode.EndWith => str.EndsWith(matchStr),
            _ => false
        };

        /// <summary>
        /// Is matchStr satisfy str ?
        /// </summary>
        /// <param name="str"></param>
        /// <param name="matchStr"></param>
        /// <param name="predication"></param>
        /// <returns></returns>
        public static bool IsMatch(this string str, string matchStr, Func<string, string, bool> predication)
        {
            if (predication == null || string.IsNullOrEmpty(str) || string.IsNullOrEmpty(matchStr))
                return false;

            return predication(str, matchStr);
        }

        /// <summary>
        /// split string by \n
        ///     k = v
        /// form dictionary 
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="dict"></param>
        /// <param name="splitChar"></param>
        public static void ReadKeyValue(this string str, Dictionary<string, string> dict, char splitChar = '\n')
        {
            if (string.IsNullOrEmpty(str) || dict == null) return;

            var lines = str.Split(splitChar);
            if (lines == null)
                return;

            foreach (var lineStr in lines)
            {
                var line = lineStr.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                    continue;

                var kv = kvRegex.Split(line);
                if (kv.Length > 1)
                    dict[kv[0]] = kv[1];
            }
        }

        /// <summary>
        /// str split then Trim string segments
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <returns></returns>
        public static string[] SplitBy(this string str, char splitChar = ',')
        {
            if (string.IsNullOrEmpty(str)) return new string[] { };
            return str.Split(splitChar)
                .Select(item => item.Trim())
                .ToArray();
        }

        /// <summary>
        /// Remove count characters from string last.
        /// 
        /// str.Substring(0, str.Length - count);
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string SubstringLast(this string str, int count)
        => str.Substring(0, str.Length - count);

        public static string ToString<T>(IEnumerable<T> items) => string.Join(',', items);

        /// <summary>
        /// is all of strs not empty
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool IsAll(params string[] strs)
        {
            return strs.All(str => !string.IsNullOrEmpty(str));
        }
        /// <summary>
        /// Is strs  has not empty item at least 1.
        /// </summary>
        /// <param name="strs"></param>
        /// <returns></returns>
        public static bool IsAny(params string[] strs)
        {
            return strs.Any(str => !string.IsNullOrEmpty(str));
        }
    }
}