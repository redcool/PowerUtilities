using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    /// <summary>
    /// Reinterpret extends 
    /// </summary>
    public static class NumberTypeReinterpret
    {
        /// <summary>
        /// reinterpret int to float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsFloat(this int value)
        => ((NumberType)value).floatValue;

        public static float AsFloat(this uint value)
            => ((NumberType)value).floatValue;
        /// <summary>
        /// reinterpret int to uint, -1 : 4.294967E+09
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint AsUint(this int value)
        => ((NumberType)value).uintValue;
        /// <summary>
        /// float -> uint
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static uint AsUInt(this float value)
        => ((NumberType)value).uintValue;

        /// <summary>
        /// float -> int
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int AsInt(this float value)
        => ((NumberType)value).intValue;

        public static int AsInt(this uint value)
        => ((NumberType)value).intValue;

    }
}
