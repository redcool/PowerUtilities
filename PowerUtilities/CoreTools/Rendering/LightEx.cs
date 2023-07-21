using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    public static class LightEx
    {
        public static bool HasShadowMask(this Light light, out int occlusionMaskChannel)
        {
            var bakingOutput = light.bakingOutput;
            occlusionMaskChannel = bakingOutput.occlusionMaskChannel;
            var hasShadowMask = bakingOutput.lightmapBakeType == LightmapBakeType.Mixed ||
                bakingOutput.mixedLightingMode == MixedLightingMode.Shadowmask;
            return hasShadowMask;
        }
    }
}
