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
        [StructLayout(LayoutKind.Explicit)]
        struct IntFloat
        {
            [FieldOffset(0)]public float floatValue;
            [FieldOffset(0)]public int intValue;

            public static implicit operator IntFloat(int v)
            {
                return new IntFloat { intValue= v };
            }
        }
        /// <summary>
        /// reinterpret int to float
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float AsFloat(this int value)
        {
            IntFloat intFloat = value;
            return intFloat.floatValue;
        }
    }
}
