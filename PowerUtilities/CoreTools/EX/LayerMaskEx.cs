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
        public static int ToLayerMask(this int objectLayer)
        {
            return 1 << objectLayer;
        }
        /// <summary>
        /// layerMask to layerIndex(gameObject.layer)
        /// </summary>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static int ToLayerId(this LayerMask layerMask)
        {
            //var uintValue = ((int)layerMask).AsUint();
            // include layer 31
            if ((layerMask & 0x100000000) != 0)
                return 31;

            return (int)Math.Log(layerMask, 2);
        }

        public static bool Contains(this LayerMask layers, int objectLayer)
        => (layers & ToLayerMask(objectLayer)) != 0;

        public static bool Contains(this LayerMask layers, LayerMask layerMask)
        => (layers & layerMask) != 0;


        public static LayerMask Remove(this LayerMask layers, LayerMask layerMask)
        => layers& ~layerMask;

        public static LayerMask Add(this LayerMask layers, LayerMask layerMask)
            => layers | layerMask;
    }
}
