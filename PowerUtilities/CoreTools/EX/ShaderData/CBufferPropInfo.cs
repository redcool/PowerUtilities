
using System;

namespace PowerUtilities
{
    /// <summary>
    /// Shader cbuffer variable info
    /// </summary>
    [Serializable]
    public class CBufferPropInfo
    {
        /// <summary>
        /// shader variable name
        /// </summary>
        public string propName;
        /// <summary>
        /// half,halfN,float,floatN,
        /// </summary>
        public string propType;


        public int propNameId;
        /// <summary>
        /// floatN
        /// </summary>
        public int floatsCount;
    }
}
