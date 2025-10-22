using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class RendererEx
    {
        /// <summary>
        /// usage : go.GetComponent<Renderer>()?.SetEnable()
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="isEnable"></param>
        /// <returns></returns>
        public static bool SetEnable(this Renderer renderer, bool isEnable)
        {
            if (!renderer) 
                return false;
            return renderer.enabled = isEnable;
        }
    }
}
