namespace PowerUtilities
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class StringEx
    {

        public readonly static Regex kvRegex = new Regex(@"\s*=\s*");

        static readonly char[] DEFAULT_SPLIT_CHARS = new []{'\n'};
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
        public static void ReadKeyValue(this string str, Dictionary<string, string> dict, string splitChars = "\n")
        {
            ReadKeyValue(str, splitChars, (kv) =>
            {
                if(kv.Length > 1)
                    dict[kv[0]] = kv[1];
            });
        }

        public static void ReadKeyValue(this string str,string splitChars= "\n", Action<string[]> onReadLineKeyValue = null)
        {
            if (onReadLineKeyValue == null)
                return;

            foreach (var line in ReadLines(str, splitChars))
            {
                if (line.StartsWith("//"))
                    continue;

                onReadLineKeyValue?.Invoke(SplitKeyValuePair(line));
            }
        }
        /// <summary>
        /// Read line from str one by one
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChars"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLines(this string str, string splitChars)
        {
            var lines = str.Split(splitChars);
            if (lines == null)
                yield break;

            foreach (var lineStr in lines)
            {
                var line = lineStr.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                yield return line;
            }
        }
        /// <summary>
        /// Use '\n' split str, yield return one by one
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<string> ReadLines(this string str)
        {
            using (var reader = new StringReader(str))
            {
                string lineStr = null;

                while ((lineStr = reader.ReadLine()) != null)
                {
                    var line = lineStr.Trim();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    yield return line;
                }
            }
        }
        /// <summary>
        /// Split key=value,to [key,value]
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static string[] SplitKeyValuePair(this string line, Regex regex=null)
        {
            regex = regex ?? kvRegex;
            return kvRegex.Split(line);
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

        public static string ToString<T>(IEnumerable<T> items) => string.Join(",", items);

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

        public static string ToBase64(this string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(this string str)
        {
            var bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }


        public static string Encrypt(this string str)
        {
            var sb = new StringBuilder(str);
            for (var i = 0; i < sb.Length; i++)
            {
                sb[i] = (char)((i % 2 != 0) ? sb[i] + 1 : sb[i]);
            }
            return sb.ToString();
        }

        public static string Decrypt(this string str)
        {
            var sb = new StringBuilder(str);
            for (var i = 0; i < sb.Length; i++)
            {
                sb[i] = (char)((i % 2 != 0) ? sb[i] - 1 : sb[i]);
            }
            return sb.ToString();
        }
    }
}