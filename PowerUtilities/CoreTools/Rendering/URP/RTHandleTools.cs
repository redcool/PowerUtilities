using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_2020
using UniversalRenderer = UnityEngine.Rendering.Universal.ForwardRenderer;
#endif

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
        //UniversalRenderer RTHandle varibles
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
        _CameraColorAttachment,
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
        /// RTHandle array count 1
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_1 = new RTHandle[1];

        /// <summary>
        /// RTHandle array count 2
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_2 = new RTHandle[2];
        /// <summary>
        /// RTHandle array count 3
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_3 = new RTHandle[3];
        /// <summary>
        /// RTHandle array count 4
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_4 = new RTHandle[4];
        /// <summary>
        /// RTHandle array count 5
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_5 = new RTHandle[5];
        /// <summary>
        /// RTHandle array count 6
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_6 = new RTHandle[6];
        /// <summary>
        /// RTHandle array count 7
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_7 = new RTHandle[7];
        /// <summary>
        /// RTHandle array count 8
        /// </summary>
        public static readonly RTHandle[] RTHandleArray_8 = new RTHandle[8];

        /// <summary>
        /// URPRTHandleNames -> UniversalRenderer's rtHandle path
        /// </summary>
        static Dictionary<URPRTHandleNames, string> urpRTHandleFieldPathDict = new Dictionary<URPRTHandleNames, string>
        {
#if UNITY_2022_1_OR_NEWER
            {URPRTHandleNames._CameraColorAttachmentA,"m_ColorBufferSystem.m_A.rtResolve" },
            {URPRTHandleNames._CameraColorAttachmentB,"m_ColorBufferSystem.m_B.rtResolve" },
#else // 2021,_CameraColorAttachmentA's path
            {URPRTHandleNames._CameraColorAttachmentA,"m_ColorBufferSystem.m_A.rt" },
            {URPRTHandleNames._CameraColorAttachmentB,"m_ColorBufferSystem.m_B.rt" },
#endif
            {URPRTHandleNames._CameraDepthAttachment,nameof(URPRTHandleNames.m_CameraDepthAttachment) },
            {URPRTHandleNames._CameraDepthTexture,nameof(URPRTHandleNames.m_DepthTexture) },
            {URPRTHandleNames._CameraOpaqueTexture,nameof(URPRTHandleNames.m_OpaqueColor) },
            {URPRTHandleNames._CameraNormalsTexture,nameof(URPRTHandleNames.m_NormalsTexture) },
            {URPRTHandleNames._MotionVectorTexture,nameof(URPRTHandleNames.m_MotionVectorColor) },
        };

        /// <summary>
        /// {rtId,rtStrName}
        /// </summary>
        public static Dictionary<int, URPRTHandleNames> urpRTIdNameDict = new Dictionary<int, URPRTHandleNames>();
        public static Dictionary<string, URPRTHandleNames> urpStrName2HandleDict = new Dictionary<string, URPRTHandleNames>();

        static RTHandleTools()
        {
            var names = Enum.GetNames(typeof(URPRTHandleNames));
            foreach (var name in names)
            {
                var rtHandeName = EnumEx.Parse<URPRTHandleNames>(name);

                urpRTIdNameDict.Add(Shader.PropertyToID(name), rtHandeName);
                urpStrName2HandleDict.Add(name, rtHandeName);
            }
        }
        /// <summary>
        /// is rtStrName UniversalRenderer's rtHanle variables ?
        /// </summary>
        /// <param fieldPath="rtStrName"></param>
        /// <returns></returns>
        public static bool IsURPRTHandleName(string rtStrName) => urpStrName2HandleDict.ContainsKey(rtStrName);

        /// <summary>
        /// return true(rtStrName niversalRenderer's rtHanle variables &&  rt is allocated)
        /// otherwise return false
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="rtStrName"></param>
        /// <returns></returns>
        public static bool IsURPRTAlloced(Camera cam, string rtStrName)
        {
            if (urpStrName2HandleDict.TryGetValue(rtStrName, out var rtHandleName))
            {
                var cameraData = cam.GetUniversalAdditionalCameraData();
                var renderer = (UniversalRenderer)cameraData.scriptableRenderer;
                //var renderer = RenderPipelineTools.UrpAsset.GetDefaultRenderer(
                var rtHandle = renderer.GetRTHandle(rtHandleName);
                return rtHandle != null && rtHandle.rt;
            }

            return false;
        }
        /// <summary>
        /// Get urp renderTexture'name by rtId
        /// 
        /// false, rtId isn't urp renderTexture
        /// </summary>
        /// <param name="rtId"></param>
        /// <param name="handleName"></param>
        /// <returns></returns>
        public static bool TryGetURPTextureName(RenderTargetIdentifier rtId, out URPRTHandleNames handleName)
        {
            var nameId = rtId.GetNameId();
            return urpRTIdNameDict.TryGetValue(nameId, out handleName);
        }

        /// <summary>
        /// Get URP's rtHandles from Renderer
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="renderer"></param>
        /// <param name="name"></param>
        public static void GetRTHandle(ref RTHandle handle, ScriptableRenderer renderer, URPRTHandleNames handleName)
        {

#if UNITY_2022_1_OR_NEWER
            // check handle and handle.rt
            if (handle != null && handle.rt)
                return;
#else
            // check handle,use temporaryrt
            if (handle != null)
                return;
#endif

            var fieldPath = DictionaryTools.Get(urpRTHandleFieldPathDict, handleName, handleName => handleName.ToString());

#if UNITY_2022_1_OR_NEWER
            handle = (RTHandle)renderer.GetObjectHierarchy(fieldPath);
#else
            var rth = (RenderTargetHandle)renderer.GetObjectHierarchy(fieldPath);
            handle = RTHandles.Alloc(rth.Identifier());
#endif
        }

        /// <summary>
        /// Get Handles.s_DefaultInstance
        /// </summary>
        public static RTHandleSystem URPDefaultRTHandleSystem => lazyGetRTHandleSystem.Value;

        static Lazy<RTHandleSystem> lazyGetRTHandleSystem = new Lazy<RTHandleSystem>(() => typeof(RTHandles).GetFieldValue<RTHandleSystem>(null, "s_DefaultInstance"));

        /// <summary>
        /// RenderTargetIdentifier to RTHandle
        /// </summary>
        public static Func<RenderTargetIdentifier, RTHandle> GetRTHandleByID = (rtId) => RTHandles.Alloc(rtId);

        public readonly static RTHandle ZeroHandle = RTHandles.Alloc(0);
        public readonly static RTHandle CameraTargetHandle = RTHandles.Alloc(BuiltinRenderTextureType.CameraTarget);
    }
}
