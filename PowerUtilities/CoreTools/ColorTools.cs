using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class ColorTools
    {
        public const string LIGHT_GREEN = "#749C75";

        public static void SetupColor(string str, ref Color c)
        {
            if (!string.IsNullOrEmpty(str))
                ColorUtility.TryParseHtmlString(str, out c);
        }
    }
}
