using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities
{
    public static class ForwardLightEx
    {
        /// <summary>
        /// get MixedLightingSetup,call this in OnCameraSetup
        /// </summary>
        /// <param name="f"></param>
        /// <param name="renderingData"></param>
        /// <returns></returns>
        public static MixedLightingSetup GetMixedlightingSetup(this ForwardLights f,ref RenderingData renderingData)
        {
            // this field,cannot work fine.
            //var setup = f.GetType().GetFieldValue<MixedLightingSetup>(f, "m_MixedLightingSetup");
            //return setup;

            /**
             renderingData.lightData.visibleLights, include 2 dir realime lights, PreRenderLight
             */
            foreach (var vl in renderingData.lightData.visibleLights)
            {
                if(vl.light.bakingOutput.lightmapBakeType == LightmapBakeType.Mixed
                    && vl.light.shadows != LightShadows.None)
                {

                    switch (vl.light.bakingOutput.mixedLightingMode)
                    {
                        case MixedLightingMode.Subtractive:
                            return MixedLightingSetup.Subtractive;
                        case MixedLightingMode.Shadowmask:
                            return MixedLightingSetup.ShadowMask;
                    }
                }
                //Debug.Log(vl.light.bakingOutput.lightmapBakeType+":"+ vl.light.shadows);
            }

            return MixedLightingSetup.None;
        }

    }
}
