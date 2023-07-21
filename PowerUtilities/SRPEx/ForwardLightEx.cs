using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities
{
    public static class ForwardLightEx
    {
        /// <summary>
        /// get MixedLightingSetup
        /// </summary>
        /// <param name="f"></param>
        /// <param name="renderingData"></param>
        /// <returns></returns>
        public static MixedLightingSetup GetMixedlightingSetup(this ForwardLights f,ref RenderingData renderingData)
        {
            // this field,cannot work fine.
            //var setup = f.GetType().GetFieldValue<MixedLightingSetup>(f, "m_MixedLightingSetup");
            //return setup;

            foreach (var vl in renderingData.lightData.visibleLights)
            {
                if(vl.light.bakingOutput.lightmapBakeType == UnityEngine.LightmapBakeType.Mixed
                    && vl.light.shadows != UnityEngine.LightShadows.None
                    )
                {
                    switch (vl.light.bakingOutput.mixedLightingMode)
                    {
                        case UnityEngine.MixedLightingMode.Subtractive:
                            return MixedLightingSetup.Subtractive;
                        case UnityEngine.MixedLightingMode.Shadowmask:
                            return MixedLightingSetup.ShadowMask;
                    }
                }
            }
            return MixedLightingSetup.None;
        }

    }
}
