using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static class StringBuilderTools
    {
        static StringBuilder inst;
        /// <summary>
        /// Gets the singleton instance of StringBuilder. If it doesn't exist, it creates a new instance.
        /// </summary>
        public static StringBuilder Instance
        {
            get
            {
                if (inst == null)
                    inst = new StringBuilder();
                return inst;
            }
        }
        public static StringBuilder Append(this StringBuilder sb, string separator = "\n", params object[] objs)
        {
            foreach (var obj in objs)
            {
                sb.Append(obj).Append(separator);
            }
            return sb;
        }

        public static StringBuilder Append(string separator = "\n", params object[] objs)
        {
            return Append(Instance, separator, objs);
        }

        public static StringBuilder InsertStart(this StringBuilder sb,string separator = "\n", params object[] objs)
        {
            foreach (var obj in objs)
            {
                sb.Insert(0, separator).Insert(0, obj);
            }
            return sb;
        }
        public static StringBuilder InsertStart(string separator = "\n", params object[] objs)
        {
            return InsertStart(Instance, separator, objs);
        }

        /// <summary>
        /// Extension method for StringBuilder to append multiple objects with a newline after each.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static StringBuilder AppendLine(this StringBuilder sb, params object[] objs)
        {
            foreach (var obj in objs)
            {
                sb.AppendLine(obj.ToString());
            }
            return sb;
        }

        public static StringBuilder AppendLine(params object[] objs)
        {
            return Instance.AppendLine(objs);
        }
        /// <summary>
        /// Get string from StringBuilder and clear it if isClearSB is true.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="isClearSB"></param>
        /// <returns></returns>
        public static string ToString(this StringBuilder sb, bool isClearSB)
        {
            if (sb == null)
                return string.Empty;
            var result = sb.ToString();
            if (isClearSB)
                sb.Clear();
            return result;
        }
        public static string ToString(bool isClearSB=true)
        {
            return ToString(Instance, isClearSB);
        }
    }
}
