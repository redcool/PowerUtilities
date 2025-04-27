using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Color tools
    /// get color check : https://coolors.co/
    /// </summary>
    public static class ColorTools
    {
        /// <summary>
        /// Nice color strings
        /// </summary>
        public const string
            //red series
            R_EGG_PLANT = "#533747",
            R_FLEX = "#E7E08B",
            R_OLD_ROSE = "#CE796B",
            R_CORAL = "#EB8258",
            R_MELON = "#FEC0AA",
            R_FLAME = "#EC4E20",
            R_CHINESE_VIOLET = "#6C596E",
            //yello
            R_MAIZE = "#E7E247",
            R_OLIVE = "#84732B",
            R_DRAB_DRAK_BROWN = "#574F2A",
            //green series
            G_LIGHT_GREEN = "#749C75",
            G_PAKISTAN_GREEN = "#1C3A13",
            // blue series
            B_STATE_GRAY = "#6F7D8C",
            B_CADET_GRAY = "#77A0A9",
            B_RIFFANY_BLUE = "#86BBBD"
            ;


        public static void SetupColor(string str, ref Color c)
        {
            if (!string.IsNullOrEmpty(str))
                ColorUtility.TryParseHtmlString(str, out c);
        }
    }
}
