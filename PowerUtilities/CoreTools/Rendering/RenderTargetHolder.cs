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
        /// {rtid , rtHandle}
        /// </summary>
        static Dictionary<RenderTargetIdentifier, RTHandle> rtIdHandleDict = new Dictionary<RenderTargetIdentifier, RTHandle>();

        /// <summary>
        /// { {length , array}}
        /// </summary>
        static Dictionary<int, RTHandle[]> placeHolderHandleDict = new Dictionary<int, RTHandle[]>();
        static Dictionary<int, RenderTargetIdentifier[]> placeHolderRtIDDict = new Dictionary<int, RenderTargetIdentifier[]>();



        /// <summary>
        /// keep these colorRTs(8),depthRT
        /// </summary>
        public static RTHandle LastDepthTargetHandle;

        public static RTHandle[] LastColorTargetHandles;// = new RTHandle[8];
        public static RenderTargetIdentifier[] LastColorTargetIds;//= new RenderTargetIdentifier[8];

        static int lastColorIdsLength = 0;

        /// <summary>
        /// cached funcs
        /// </summary>
        static Func<int, RenderTargetIdentifier[]> GetIDArray = (lengthAsKey) => new RenderTargetIdentifier[lengthAsKey];
        static Func<int, RTHandle[]> GetRTHandleArray = (lengthAsKey) => new RTHandle[lengthAsKey];



        /// <summary>
        /// Save current targets, sfcpass can reuse these
        /// </summary>
        /// <param name="colorIds"></param>
        /// <param name="depthId"></param>
        public static void SaveTargets(RenderTargetIdentifier[] colorIds, RenderTargetIdentifier depthId)
        {
            LastDepthTargetHandle = DictionaryTools.Get(rtIdHandleDict, depthId, RTHandleTools.GetRTHandleByID);
            lastColorIdsLength = colorIds.Length;

            // 1 get target array
            LastColorTargetIds = DictionaryTools.Get(placeHolderRtIDDict, lastColorIdsLength, GetIDArray);
            LastColorTargetHandles = DictionaryTools.Get(placeHolderHandleDict, lastColorIdsLength, GetRTHandleArray);

            // 2 fill target array
            for (int i = 0; i < lastColorIdsLength; i++)
            {
                var rtId = colorIds[i];
                LastColorTargetIds[i] = rtId;
                LastColorTargetHandles[i] = DictionaryTools.Get(rtIdHandleDict, rtId, RTHandleTools.GetRTHandleByID);
            }

        }

        /// <summary>
        /// Exists target?
        /// </summary>
        /// <returns></returns>
        public static bool IsLastTargetValid() => lastColorIdsLength > 0 && LastDepthTargetHandle != null;

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
                RTHandleArray_1[0] = colorTarget;

                colorIds = RTHandleArray_1;
                depthId = depthTarget;
            }
        }
    }
}
