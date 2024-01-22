#if UNITY_2020
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [AttributeUsage(AttributeTargets.All)]
    public class TooltipAttribute : UnityEngine.TooltipAttribute
    {
        public TooltipAttribute(string tooltip) : base(tooltip)
        {
        }
    }
}
#endif