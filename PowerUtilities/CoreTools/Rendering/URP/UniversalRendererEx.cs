using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowerUtilities
{
    /// <summary>
    /// UniversalRenderer compatible extends
    /// </summary>
    public static class UniversalRendererEx
    {
        static RTHandle
            _CameraColorAttachmentA,
            _CameraColorAttachmentB
            ;

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
