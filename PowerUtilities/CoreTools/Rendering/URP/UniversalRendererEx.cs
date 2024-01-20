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
    /// <summary>
    /// Extends UniversalRenderer,special for private RTHandles
    /// </summary>
    public static partial class UniversalRendererEx 
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
        /// {rthandleName : URPRTHandleNames}
        /// </summary>
        static Dictionary<string, URPRTHandleNames> handleNameToEnumDict = new Dictionary<string, URPRTHandleNames>();

        /// <summary>
        /// Get urp private rtHandle
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="rtName"></param>
        /// <returns></returns>
        public static RTHandle GetRTHandle(this UniversalRenderer renderer, URPRTHandleNames rtName)
        {
            urpRTHandleDict.TryGetValue(rtName, out var handle);
            //this function with cache.
            RTHandleTools.GetRTHandle(ref handle, renderer, rtName);
            // save or again
            urpRTHandleDict[rtName] = handle;
            return handle;
        }

        /// <summary>
        /// Get urp renderTarget and cache it,
        /// when rtHandle changed will get it again.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="rtName"></param>
        /// <returns></returns>
        public static RTHandle GetRenderTargetId(this UniversalRenderer renderer, URPRTHandleNames rtName)
        {
            var handle = GetRTHandle(renderer, rtName);
            return handle;
        }

        /// <summary>
        /// if name is URP renderTarget, replace id to urp renderTarget
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// 
        static Func<string,URPRTHandleNames> ParseNameFunc = (string name) => Enum.Parse<URPRTHandleNames>(name);
        public static void TryReplaceURPRTTarget(this UniversalRenderer renderer, string name, ref RenderTargetIdentifier id)
        {
            if (string.IsNullOrEmpty(name))
                return;

            if (RTHandleTools.IsURPRTHandleName(name))
            {
                if (!handleNameToEnumDict.TryGetValue(name, out var handle))
                    handle = handleNameToEnumDict[name] = Enum.Parse<URPRTHandleNames>(name);

                //var handle = DictionaryTools.Get(handleNameToEnumDict, name, ParseNameFunc);

                id = renderer.GetRenderTargetId(handle);
                // urp not alloc it, use nameId
                if (id == default)
                    id = Shader.PropertyToID(name);
            }
        }

        /// <summary>
        /// if rtId's nameId is urp renderTarget,replace rtId
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="rtId"></param>
        public static void TryReplaceURPRTTarget(this UniversalRenderer renderer, ref RenderTargetIdentifier rtId)
        {
            if(RTHandleTools.TryGetURPTextureName(rtId, out var rtName))
            {
                rtId = GetRenderTargetId(renderer, rtName);
            }
        }
        /// <summary>
        /// check rt name, if it is UnviersalRenderer's rtHandle, replace to urp rtHanlde
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

        public static RTHandle GetCameraColorAttachmentA(this UniversalRenderer renderer)
        => renderer.GetRTHandle(URPRTHandleNames._CameraColorAttachmentA);

        public static RTHandle GetCameraColorAttachmentB(this UniversalRenderer renderer)
        => renderer.GetRTHandle(URPRTHandleNames._CameraColorAttachmentB);

        public static RTHandle GetActiveCameraColorAttachment(this UniversalRenderer renderer)
        => renderer.GetRTHandle(URPRTHandleNames.m_ActiveCameraColorAttachment);

        public static RTHandle GetActiveCameraDepthAttachment(this UniversalRenderer renderer)
        => renderer.GetRTHandle(URPRTHandleNames.m_ActiveCameraDepthAttachment);

        public static RTHandle GetCameraDepthTexture(this UniversalRenderer renderer)
        => renderer.GetRTHandle(URPRTHandleNames.m_DepthTexture);

        public static RTHandle GetCameraOpaqueTexture(this UniversalRenderer renderer)
        => renderer.GetRTHandle(URPRTHandleNames._CameraOpaqueTexture);
    }

}
