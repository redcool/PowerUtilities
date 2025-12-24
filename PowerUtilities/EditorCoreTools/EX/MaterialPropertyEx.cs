#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace PowerUtilities
{

    public static class MaterialPropertyEx
    {
        /// <summary>
        /// return PropType or ShaderPropertyType
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static int GetPropertyType(this MaterialProperty prop) 
        {
#if UNITY_6000_1_OR_NEWER
            return (int)prop.propertyType;
#else
            return (int)prop.type;
#endif
        }

        public static int GetPropertyFlags(this MaterialProperty prop)
        {
#if UNITY_6000_1_OR_NEWER
            return (int)prop.propertyFlags;
#else
            return (int)prop.flags;
#endif
        }
    }


}

#endif