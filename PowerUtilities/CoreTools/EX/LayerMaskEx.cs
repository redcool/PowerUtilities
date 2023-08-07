using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class LayerMaskEx
    {
        /// <summary>
        /// gameObject's layer to LayerMask
        /// </summary>
        /// <param name="objectLayer"></param>
        /// <returns></returns>
        public static int ToMask(int objectLayer)
        {
            return 1 << objectLayer;
        }

        public static bool Contains(this LayerMask layers, int objectLayer)
        => (layers & ToMask(objectLayer)) > 0;

        public static bool Contains(this LayerMask layers, LayerMask layerMask)
        => (layers & layerMask) > 0;


        public static LayerMask Remove(this LayerMask layers, LayerMask layerMask)
        => layers& ~layerMask;

        public static LayerMask Add(this LayerMask layers, LayerMask layerMask)
            => layers | layerMask;
    }
}
