namespace PowerUtilities
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class StringEx
    {
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

        public static bool IsMatch(this string str, string matchStr, Func<string, string, bool> predication)
        {
            if (predication == null || string.IsNullOrEmpty(str) || string.IsNullOrEmpty(matchStr))
                return false;

            return predication(str, matchStr);
        }
    }
}