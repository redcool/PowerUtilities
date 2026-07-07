using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class StringBuilderTools
    {
        static StringBuilder sb;
        public static StringBuilder Instance
        {
            get
            {
                if (sb == null)
                    sb = new StringBuilder();
                return sb;
            }
        }
        public static StringBuilder Append(string separator = "\n", bool isClearSB = false, params object[] objs)
        {
            if (sb == null)
                sb = new StringBuilder();
            if (isClearSB)
                sb.Clear();
            foreach (var obj in objs)
            {
                sb.Append(obj).Append(separator);
            }
            return sb;
        }
        public static string ToString(bool isClearSB=true)
        {
            if (sb == null)
                return string.Empty;
            var result = sb.ToString();
            if (isClearSB)
                sb.Clear();
            return result;
        }
        /// <summary>
        /// Appends multiple objects to the (Singleton StringBuilder )with a newline after each.
        /// </summary>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static StringBuilder AppendLine(params object[] objs)
        {
            if (sb == null)
                sb = new StringBuilder();
            foreach (var obj in objs)
            {
                sb.AppendLine(obj.ToString());
            }
            return sb;
        }
        /// <summary>
        /// Extension method for StringBuilder to append multiple objects with a newline after each.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static StringBuilder AppendLine(this StringBuilder sb, params object[] objs)
        {
            if (sb == null)
                sb = new StringBuilder();
            foreach (var obj in objs)
            {
                sb.Append(obj).Append("\n");
            }
            return sb;
        }


    }
}
