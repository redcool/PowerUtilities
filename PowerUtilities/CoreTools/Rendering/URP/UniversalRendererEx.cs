using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace PowerUtilities
{
    public static class UniversalRendererEx
    {
        static CacheTool<UniversalRenderer, ForwardLights> rendererForwardLightsCache = new CacheTool<UniversalRenderer, ForwardLights>();


        /// <summary>
        /// Get ForwardLights use reflection
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static ForwardLights GetForwardLights(this UniversalRenderer r)
        {
            return rendererForwardLightsCache.Get(r, () => r.GetType().GetFieldValue<ForwardLights>(r, "m_ForwardLights"));
        }

        /// <summary>
        /// handleName -> RTHandle
        /// </summary>
        static Dictionary<URPRTHandleNames, RTHandle> urpRTHandleDict = new Dictionary<URPRTHandleNames, RTHandle>();

        /// <summary>
        /// Get urp renderTarget and cache it,
        /// when rtHandle changed will get it again.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="rtIdName"></param>
        /// <returns></returns>
        public static RenderTargetIdentifier GetRenderTargetId(this UniversalRenderer renderer, URPRTHandleNames rtIdName)
        {
            if (!urpRTHandleDict.TryGetValue(rtIdName, out var handle))
            {
                urpRTHandleDict[rtIdName] = handle;
            }
            RTHandleTools.GetRTHandle(ref handle, renderer, rtIdName);
            return handle.nameID;
        }

        public static void TryReplaceURPRTTarget(this UniversalRenderer renderer, string name, ref RenderTargetIdentifier id)
        {
            if (RTHandleTools.IsURPRTHandleName(name))
                id = renderer.GetRenderTargetId(Enum.Parse<URPRTHandleNames>(name));
        }

        public static void TryReplaceURPRTTarget(this UniversalRenderer renderer, ref RenderTargetIdentifier rtId)
        {
            if(RTHandleTools.TryGetURPTextureName(rtId, out var rtName))
            {
                rtId = GetRenderTargetId(renderer, rtName);
            }
        }
        /// <summary>
        /// check rt name, if it is UnviersalRenderer's rtHandle, use urp rtHanlde
        /// </summary>
        /// <param name="names"></param>
        /// <param name="ids"></param>
        public static void TryReplaceURPRTTargets(this UniversalRenderer renderer, string[] names, ref RenderTargetIdentifier[] ids)
        {
            for (int i = 0; i < names.Length; i++)
            {
                TryReplaceURPRTTarget(renderer, names[i], ref ids[i]);
            }
        }

        
    }
}
