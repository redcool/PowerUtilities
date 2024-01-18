using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Application exit call
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApplicationExitAttribute : Attribute
    {
        public static HashSet<Type> instTypeList = new HashSet<Type>();
        public Type type;

        public ApplicationExitAttribute(Type instType)
        {
            type = instType;
        }
    }


}
