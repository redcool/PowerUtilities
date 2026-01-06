using System.Runtime.InteropServices;

namespace PowerUtilities
{
    /// <summary>
    /// Reinterprec type for any number(int,float,uint),
    /// 
    /// int to uint:
    /// new NumberType { intValue = -1}.uintValue
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct NumberType
    {
        [FieldOffset(0)] public float floatValue;
        [FieldOffset(0)] public int intValue;
        [FieldOffset(0)] public uint uintValue;

        public static implicit operator NumberType(int v)
            => new NumberType { intValue = v };
        public static implicit operator NumberType(uint v)
            => new NumberType { uintValue = v };
        public static implicit operator NumberType(float v)
            => new NumberType { floatValue = v };
    }
}
