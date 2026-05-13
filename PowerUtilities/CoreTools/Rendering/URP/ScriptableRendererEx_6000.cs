#if UNITY_6000_4_OR_NEWER
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// handle ScriptableRenderer(UniversalRenderer) passes by reflections
    /// </summary>
    public static partial class ScriptableRendererEx
    {

        /// <summary>
        /// 6000 cannot get activeColorTexture and activeDepthTexture from m_frameData,
        /// set null will use frameData's activeColorTexture and activeDepthTexture, so return default here.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static RTHandle CameraColorTargetHandle(this ScriptableRenderer r)
        {
            return default;
        }
        /// <summary>
        /// 6000 cannot get activeColorTexture and activeDepthTexture from m_frameData,
        /// set null will use frameData's activeColorTexture and activeDepthTexture, so return default here.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static RTHandle CameraDepthTargetHandle(this ScriptableRenderer r)
        {
            return default;
        }

    }
}
#endif