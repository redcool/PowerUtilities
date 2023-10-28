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
    /// UniversalRenderer RTHandle variableName,check UniversalRenderer.cs
    /// 
    /// URP's rt use RTHandle(private) , 
    /// sfc framework use propertyName(Shader.Shader.PropertyToID
    /// </summary>
    public enum URPRTHandleNames
    {
        //UniversalRenderer RTHandles
        m_ActiveCameraColorAttachment,
        m_ActiveCameraDepthAttachment,
        m_CameraDepthAttachment,
        m_XRTargetHandleAlias,
        m_XRDepthHandleAlias,
        m_DepthTexture,
        m_NormalsTexture,
        m_DecalLayersTexture,
        m_OpaqueColor,
        m_MotionVectorColor,
        m_MotionVectorDepth,

        // also shader property
        _CameraColorAttachmentA,
        _CameraColorAttachmentB,
        _CameraDepthAttachment,
        _CameraDepthTexture,
        _CameraOpaqueTexture,
        _CameraNormalsTexture,
        _MotionVectorTexture,
    }

    public static class RTHandleTools
    {
        
        /// <summary>
        /// urp texture property name -> UniversalRenderer's rtHandle path
        /// </summary>
        static Dictionary<string, string> rtHandleFieldPathDict = new Dictionary<string, string>
        {
#if UNITY_2023_1_OR_NEWER
            {nameof(URPRTHandleNames._CameraColorAttachmentA),"m_ColorBufferSystem.m_A.rtResolve" },
            {nameof(URPRTHandleNames._CameraColorAttachmentB),"m_ColorBufferSystem.m_B.rtResolve" },
#else // 2021,_CameraColorAttachmentA's path
            {nameof(URPRTHandleNames._CameraColorAttachmentA),"m_ColorBufferSystem.m_A.rt" },
            {nameof(URPRTHandleNames._CameraColorAttachmentB),"m_ColorBufferSystem.m_B.rt" },
#endif

            {nameof(URPRTHandleNames._CameraDepthAttachment),nameof(URPRTHandleNames.m_CameraDepthAttachment) },
            {nameof(URPRTHandleNames._CameraDepthTexture),nameof(URPRTHandleNames.m_DepthTexture) },
            {nameof(URPRTHandleNames._CameraOpaqueTexture),nameof(URPRTHandleNames.m_OpaqueColor) },
            {nameof(URPRTHandleNames._CameraNormalsTexture),nameof(URPRTHandleNames.m_NormalsTexture) },
            {nameof(URPRTHandleNames._MotionVectorTexture),nameof(URPRTHandleNames.m_MotionVectorColor) },
        };


        /// <summary>
        /// is rtName UniversalRenderer's rtHanle variables ?
        /// </summary>
        /// <param fieldPath="rtName"></param>
        /// <returns></returns>
        public static bool IsURPRTHandleName(string rtName) => Enum.IsDefined(typeof(URPRTHandleNames), rtName);
 
        /// <summary>
        /// Get RTHandle from Renderer
        /// </summary>
        /// <param fieldPath="rth"></param>
        /// <param fieldPath="renderer"></param>
        /// <param fieldPath="fieldPath"> check RTHandleNames </param>
        public static void GetRTHandle(ref RTHandle rth, ScriptableRenderer renderer, string fieldPath)
        {
            if (rth != null && rth.rt)
                return;
            
        // get variable's path or use URPRTHandleNames
        if(rtHandleFieldPathDict.ContainsKey(fieldPath))
        {
            fieldPath = rtHandleFieldPathDict[fieldPath];
        }

#if UNITY_2023_1_OR_NEWER
            rth = (RTHandle)renderer.GetObjectHierarchy(fieldPath);
#else
            Debug.Log(fieldPath);
            var handle = (RenderTargetHandle)renderer.GetObjectHierarchy(fieldPath);
            rth = RTHandles.Alloc(handle.Identifier());
#endif
        }

        public static void GetRTHandle(ref RTHandle handle, ScriptableRenderer renderer, URPRTHandleNames name)
        {
            GetRTHandle(ref handle, renderer, Enum.GetName(typeof(URPRTHandleNames), name));
        }

        /// <summary>
        /// Get Handles.s_DefaultInstance
        /// </summary>
        public static RTHandleSystem URPDefaultRTHandleSystem => lazyGetRTHandleSystem.Value;
        static Lazy<RTHandleSystem> lazyGetRTHandleSystem = new Lazy<RTHandleSystem>(() => typeof(RTHandles).GetFieldValue<RTHandleSystem>(null, "s_DefaultInstance"));

        public static void GetRTHandleA_B(ref RTHandle handleA, ref RTHandle handleB, ScriptableRenderer renderer)
        {
            if (handleA == null || !handleA.rt)
                handleA = (RTHandle)renderer.GetObjectHierarchy("m_ColorBufferSystem.m_A.rtResolve");

            if (handleA == null || !handleA.rt)
                handleB = (RTHandle)renderer.GetObjectHierarchy("m_ColorBufferSystem.m_B.rtResolve");
        }
    }
}
