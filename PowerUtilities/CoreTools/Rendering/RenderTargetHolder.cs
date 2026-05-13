using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static PowerUtilities.RTHandleTools;

namespace PowerUtilities
{
    /// <summary>
    /// Hold render targets
    /// </summary>
    public static class RenderTargetHolder
    {

        /// <summary>
        /// keep these colorRTs(8),depthRT
        /// </summary>
        public static RTHandle LastDepthTargetHandle;

        public static RTHandle[] LastColorTargetHandles;// = new RTHandle[8];
        public static RenderTargetIdentifier[] LastColorTargetIds;//= new RenderTargetIdentifier[8];

        /// <summary>
        /// same as LastColorTargetHandles.length
        /// </summary>
        static int lastColorIdsLength = 0;

        /// <summary>
        /// Save current targets, sfcpass can reuse these
        /// </summary>
        /// <param name="colorIds"></param>
        /// <param name="depthId"></param>
        public static void SaveTargets(RenderTargetIdentifier[] colorIds, RenderTargetIdentifier depthId)
        {
            LastDepthTargetHandle = GetRTHandleByID(depthId);
            lastColorIdsLength = colorIds.Length;

            // 1 get target array
            LastColorTargetIds = GetRenderTargetIdentifiers(lastColorIdsLength);
            LastColorTargetHandles = GetRTHandles(lastColorIdsLength);

            // 2 fill target array
            for (int i = 0; i < lastColorIdsLength; i++)
            {
                var rtId = colorIds[i];
                LastColorTargetIds[i] = rtId;
                LastColorTargetHandles[i] = GetRTHandleByID(rtId);
            }

        }

        public static void SaveTargets(RTHandle[] colorHandles, RTHandle depthHandle)
        {
            LastDepthTargetHandle = depthHandle;
            lastColorIdsLength = colorHandles.Length;
            // 1 get target array
            LastColorTargetHandles = GetRTHandles(lastColorIdsLength);
            LastColorTargetIds = GetRenderTargetIdentifiers(lastColorIdsLength);
            // 2 fill target array
            for (int i = 0; i < lastColorIdsLength; i++)
            {
                var rtHandle = colorHandles[i];
                LastColorTargetHandles[i] = rtHandle;
                LastColorTargetIds[i] = rtHandle != null ? rtHandle.nameID : default;
            }
        }

        /// <summary>
        /// Exists target?
        /// </summary>
        /// <returns></returns>
        public static bool IsLastTargetValid() => lastColorIdsLength > 0 ;

        /// <summary>
        /// clear last targets
        /// </summary>
        public static void Clear()
        {
            LastColorTargetHandles = default;
            LastDepthTargetHandle = default;
            lastColorIdsLength = 0;
        }

        public static RTHandle LastColorTargetHandle => IsLastTargetValid() ? LastColorTargetHandles[0] : default;

        /// <summary>
        /// Get renderer's target or SFC last saved Targets
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="colorIds"></param>
        /// <param name="depthId"></param>
        public static void GetLastTargets(UniversalRenderer renderer, out RTHandle[] colorIds, out RTHandle depthId)
        {
            var colorTarget = renderer.CameraColorTargetHandle();
            var depthTarget = renderer.CameraDepthTargetHandle();

            if (IsLastTargetValid())
            {
                colorIds = LastColorTargetHandles;
                depthId = LastDepthTargetHandle;
            }
            else
            {
                // use rthandle array ,1 item
                colorIds = GetRTHandles(1);
                colorIds[0] = colorTarget;

                depthId = depthTarget;
            }
        }
    }
}
