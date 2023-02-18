#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class GroupAPITools
    {
        /// <summary>
        /// Translate "a12.34_10 0_1.23 0_1 0_1" to Vector2[]
        /// </summary>
        /// <param name="rangeString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Vector2[] TranslateRangeStr(string rangeString)
        {
            if (string.IsNullOrEmpty(rangeString))
                throw new ArgumentNullException(nameof(rangeString));

            const string pattern = @"([a-zA-Z]?)(\d+\.?\d*)";
            var ms = Regex.Matches(rangeString, pattern);
            
            var count = ms.Count/2;
            var items = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                var a = StrToFloat(ms[i*2]);
                var b = StrToFloat(ms[i*2+1]);
                items[i].Set(a, b);
            }
            return items;
        }
        static float StrToFloat(Match m)
        {
            float num = Convert.ToSingle(m.Groups[2].Value);

            // negative
            if (!string.IsNullOrEmpty(m.Groups[1].Value))
                num *= -1;
            return num;
        }
    }
}
#endif