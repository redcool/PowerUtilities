using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// get from UniversalRenderer.cs
    /// </summary>
    public enum URPRTHandleNames
    {
        m_ActiveCameraColorAttachment,
        m_ColorFrontBuffer,
        m_ActiveCameraDepthAttachment,
        m_CameraDepthAttachment,
        m_XRTargetHandleAlias,
        m_XRDepthHandleAlias,
        m_DepthTexture,
        m_NormalsTexture,
        m_DecalLayersTexture,
        m_OpaqueColor,
        m_MotionVectorColor,
        m_MotionVectorDepth
    }

    public static class RTHandleTools
    {
        static Dictionary<int,RTHandle> handleDict = new Dictionary<int,RTHandle>();

        /// <summary>
        /// Get or new a RTHandle from rtId(Shader.PropertyToID
        /// </summary>
        /// <param name="rtId"></param>
        /// <returns></returns>
        public static RTHandle TryGetRTHandle(int rtId,bool forceNew=false)
        {
            if(!handleDict.TryGetValue(rtId, out var rthandle) || forceNew)
            {
                rthandle = handleDict[rtId] = RTHandles.Alloc(rtId);
            }
            return rthandle;
        }

        /// <summary>
        /// Get RTHandle from Renderer
        /// </summary>
        /// <param name="rth"></param>
        /// <param name="renderer"></param>
        /// <param name="name"> check RTHandleNames </param>
        public static void TryGetRTHandle(ref RTHandle rth, ScriptableRenderer renderer, string name)
        {
            if (rth != null && rth.rt)
            {
                return;
            }

            var rendererType = renderer.GetType();
            var f = rendererType.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
#if UNITY_2023_1_OR_NEWER
            rth = (RTHandle)f.GetValue(renderer);
#else
            var handle = (RenderTargetHandle)f.GetValue(renderer);
            rth = RTHandles.Alloc(handle.Identifier());
#endif
        }

        public static void TryGetRTHandle(ref RTHandle handle, ScriptableRenderer renderer, URPRTHandleNames name) 
        {
            TryGetRTHandle(ref handle, renderer, Enum.GetName(typeof(URPRTHandleNames),name));
        }
    }
}
