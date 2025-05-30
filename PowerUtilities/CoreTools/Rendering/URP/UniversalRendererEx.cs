using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif
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
        /// <param strName="r"></param>
        /// <returns></returns>
        public static ForwardLights GetForwardLights(this UniversalRenderer r)
        {
            return rendererForwardLightsCache.Get(r, () => r.GetType().GetFieldValue<ForwardLights>(r, "m_ForwardLights"));
        }

        static Dictionary<ScriptableRenderer, ScriptableRendererRTHandleInfo> rendererRTHandleInfoDict = new Dictionary<ScriptableRenderer, ScriptableRendererRTHandleInfo>();

        static UniversalRendererEx()
        {
            ApplicationTools.OnDomainUnload -= ClearCachedRTHandles;
            ApplicationTools.OnDomainUnload += ClearCachedRTHandles;

            //RenderPipelineManager.endFrameRendering -= RenderPipelineManager_endFrameRendering;
            //RenderPipelineManager.endFrameRendering += RenderPipelineManager_endFrameRendering;

            RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
            RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;

            ScreenTools.OnScreenSizeChanged -= ClearCachedRTHandles;
            ScreenTools.OnScreenSizeChanged += ClearCachedRTHandles;
        }

        private static void RenderPipelineManager_endCameraRendering(ScriptableRenderContext arg1, Camera arg2)
        {
            var cameraData = arg2.GetUniversalAdditionalCameraData();
            ClearActiveCameraColorAttachmentCache(cameraData.scriptableRenderer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param strName="arg1"></param>
        /// <param strName="arg2"></param>
        private static void RenderPipelineManager_endFrameRendering(ScriptableRenderContext arg1, Camera[] arg2)
        {
            // m_ActiveCameraColorAttachment need get per frame
            foreach (var renderer in rendererRTHandleInfoDict.Keys)
            {
                ClearActiveCameraColorAttachmentCache(renderer);
            }
        }

        public static void ClearActiveCameraColorAttachmentCache(ScriptableRenderer renderer)
        {
            if (rendererRTHandleInfoDict.TryGetValue(renderer, out var info))
            {
                info.rtHandleDict[URPRTHandleNames.m_ActiveCameraColorAttachment] = null;
            }
        }

        public static void ClearCachedRTHandles()
        {
            rendererRTHandleInfoDict.Clear();
        }

        /// <summary>
        /// Get urp private rtHandleName
        /// if rtName -> RTHandle dont exist return default
        /// </summary>
        /// <param strName="renderer"></param>
        /// <param strName="rtName"></param>
        /// <param strName="forceMode"></param>
        /// <returns></returns>
        public static RTHandle GetRTHandle(this UniversalRenderer renderer, URPRTHandleNames rtName, bool forceMode = false)
        {
            // get renderer target
            if (rtName == URPRTHandleNames._CameraDepthAttachment ||
                rtName == URPRTHandleNames.m_CameraDepthAttachment ||
                rtName == URPRTHandleNames.m_ActiveCameraDepthAttachment)
                return renderer.CameraDepthTargetHandle();

            if (rtName == URPRTHandleNames._CameraColorAttachmentA ||
                rtName == URPRTHandleNames.m_ActiveCameraColorAttachment)
                return renderer.CameraColorTargetHandle();

            // get others
            if (!rendererRTHandleInfoDict.TryGetValue(renderer, out var handleInfo))
            {
                handleInfo = rendererRTHandleInfoDict[renderer] = new ScriptableRendererRTHandleInfo();
            }

            handleInfo.rtHandleDict.TryGetValue(rtName, out var handle);
            if (forceMode)
            {
                handle = default;
            }
            else
            {
                // checked already(rt in dict), urp rt not exists
                if (handleInfo.rtHandleDict.ContainsKey(rtName) && handle == default)
                    return default;
            }

            //this function with cache.
            RTHandleTools.GetRTHandle(ref handle, renderer, rtName);
            // save again
            handleInfo.rtHandleDict[rtName] = handle;
            return handle;
        }

        /// <summary>
        /// Get urp renderTarget and cache it,
        /// when rtHandleName changed will get it again.
        /// </summary>
        /// <param strName="renderer"></param>
        /// <param strName="rtName"></param>
        /// <returns></returns>
        public static RTHandle GetRenderTargetId(this UniversalRenderer renderer, URPRTHandleNames rtName)
        {
            var handle = GetRTHandle(renderer, rtName);
            return handle;
        }

        /// <summary>
        /// if strName is URP renderTarget, replace id to urp renderTarget,
        /// 
        /// can call ShaderPropertyIds.PropertyToID(rtName) also,its use default Renderer
        /// </summary>
        /// <param strName="renderer"></param>
        /// <param strName="name"></param>
        /// <param strName="id"></param>
        /// 
        public static bool TryReplaceURPRTTarget(this UniversalRenderer renderer, string strName, ref RenderTargetIdentifier id)
        {
            if (string.IsNullOrEmpty(strName))
                return false;

            if (RTHandleTools.urpStrName2HandleDict.TryGetValue(strName, out var rtHandleName))
            {
                id = renderer.GetRenderTargetId(rtHandleName);
                // urp not alloc it, use nameId
                if (id == default)
                    id = Shader.PropertyToID(strName);

                return id != default;
            }
            return false;
        }

        /// <summary>
        /// if rtId's nameId is urp renderTarget,replace rtId
        /// </summary>
        /// <param strName="renderer"></param>
        /// <param strName="rtId"></param>
        public static void TryReplaceURPRTTarget(this UniversalRenderer renderer, ref RenderTargetIdentifier rtId)
        {
            if (RTHandleTools.TryGetURPTextureName(rtId, out var rtName))
            {
                var urpId = GetRenderTargetId(renderer, rtName);
                if (urpId != default)
                    rtId = urpId;
            }
        }
        /// <summary>
        /// check rt strName, if it is UnviersalRenderer's rtHandleName, replace to urp rtHanlde
        /// </summary>
        /// <param strName="names"></param>
        /// <param strName="ids"></param>
        public static void TryReplaceURPRTTargets(this UniversalRenderer renderer, string[] names, ref RenderTargetIdentifier[] ids)
        {
            for (int i = 0; i < names.Length; i++)
            {
                TryReplaceURPRTTarget(renderer, names[i], ref ids[i]);
            }
        }

        public static RTHandle GetCameraColorAttachmentA(this UniversalRenderer renderer, bool forceMode = false)
        => renderer.GetRTHandle(URPRTHandleNames._CameraColorAttachmentA, forceMode);

        public static RTHandle GetCameraColorAttachmentB(this UniversalRenderer renderer, bool forceMode = false)
        => renderer.GetRTHandle(URPRTHandleNames._CameraColorAttachmentB, forceMode);

        public static RTHandle GetActiveCameraColorAttachment(this UniversalRenderer renderer, bool forceMode = false)
        //=> renderer.GetRTHandle(URPRTHandleNames.m_ActiveCameraColorAttachment, forceMode);
        => renderer.CameraColorTargetHandle();

        public static RTHandle GetActiveCameraDepthAttachment(this UniversalRenderer renderer, bool forceMode = false)
        => renderer.CameraDepthTargetHandle();

        public static RTHandle GetCameraDepthTexture(this UniversalRenderer renderer, bool forceMode = false)
        => renderer.GetRTHandle(URPRTHandleNames.m_DepthTexture, forceMode);

        public static RTHandle GetCameraDepthAttachment(this UniversalRenderer renderer, bool forceMode = false)
        => renderer.GetRTHandle(URPRTHandleNames._CameraDepthAttachment, forceMode);

        public static RTHandle GetCameraOpaqueTexture(this UniversalRenderer renderer, bool forceMode = false)
        => renderer.GetRTHandle(URPRTHandleNames._CameraOpaqueTexture, forceMode);

        /// <summary>
        /// Find rtId from
        /// 1 URP'S RTHandle dict
        /// 2 RenderTextureTools dict(createRenderTarget)
        /// </summary>
        /// <param name="rtName"></param>
        /// <param name="rtid"></param>
        public static void FindTarget(this UniversalRenderer renderer, string rtName, ref RenderTargetIdentifier rtid)
        {
            if (string.IsNullOrEmpty(rtName))
                return;

#if UNITY_2022_1_OR_NEWER
            renderer.TryReplaceURPRTTarget(rtName, ref rtid);
#endif
            if (RenderTextureTools.TryGetRT(rtName, out var rt))
                rtid = rt;
        }
    }

}
