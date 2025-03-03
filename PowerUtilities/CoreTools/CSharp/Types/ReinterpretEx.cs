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
    public static class ReinterpretEx
    {
        /// <summary>
        /// reinterpret int to float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsFloat(this int value)
        {
            NumberType intFloat = value;
            return intFloat.floatValue;
        }
        /// <summary>
        /// reinterpret int to uint, -1 : 4.294967E+09
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsUint(this int value)
        {
            NumberType intFloat = value;
            return intFloat.uintValue;
        }
    }
}
