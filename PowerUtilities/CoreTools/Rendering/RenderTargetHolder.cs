using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

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
        public static RTHandle[] LastColorTargetRTs;
        public static RTHandle LastDepthTargetRT;
        public static RenderTargetIdentifier[] LastColorTargetIds;

        /// <summary>
        /// Save current targets, sfcpass can reuse these
        /// </summary>
        /// <param name="colorIds"></param>
        /// <param name="depthId"></param>
        public static void SaveTargets(RenderTargetIdentifier[] colorIds,RenderTargetIdentifier depthId)
        {
            LastColorTargetIds = new RenderTargetIdentifier[colorIds.Length];
            LastColorTargetRTs = new RTHandle[colorIds.Length];
            for (int i = 0; i < colorIds.Length; i++)
            {
                LastColorTargetIds[i] = colorIds[i];
                LastColorTargetRTs[i] = RTHandles.Alloc(colorIds[i]);
            }

            LastDepthTargetRT = RTHandles.Alloc(depthId);
        }

        /// <summary>
        /// Exists target?
        /// </summary>
        /// <returns></returns>
        public static bool IsLastTargetValid() => LastColorTargetRTs != null && LastColorTargetRTs.Length > 0 && LastDepthTargetRT != null;

        /// <summary>
        /// clear last targets
        /// </summary>
        public static void Clear()
        {
            LastColorTargetRTs = default;
            LastDepthTargetRT = default;
        }

        public static RTHandle LastColorTargetRT => IsLastTargetValid() ? LastColorTargetRTs[0] : default;
    }
}
