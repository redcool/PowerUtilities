using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;

namespace PowerUtilities
{
    public static class SHTools
    {
        /// <summary>
        /// add Light to sh L2
        /// </summary>
        /// <param name="light"></param>
        /// <param name="sh_L2"></param>
        /// <param name="shPos"></param>
        public static void AddLight(this ref SphericalHarmonicsL2 sh_L2,Light light, Vector3 shPos)
        {
            var intensity = light.intensity;
            var dir = -light.transform.forward;
            if (light.type == LightType.Point)
            {
                dir = light.transform.position - shPos;
                intensity *= 1f / (1 + 25 * (dir.sqrMagnitude / light.range / light.range));
                dir.Normalize();
            }
            sh_L2.AddDirectionalLight(dir, light.color, intensity);
        }
    }
}
