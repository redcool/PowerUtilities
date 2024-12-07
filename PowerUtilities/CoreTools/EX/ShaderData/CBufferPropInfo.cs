
using System;

namespace PowerUtilities
{
    /// <summary>
    /// Shader cbuffer variable info
    /// </summary>
    [Serializable]
    public class CBufferPropInfo
    {
        public string propName;
        public string propType;
        public int floatsCount;
    }
}
