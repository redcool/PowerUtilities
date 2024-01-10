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
        public static RTHandle LastDepthTargetHandle;

        public static RTHandle[] LastColorTargetHandles = new RTHandle[8];
        public static RenderTargetIdentifier[] LastColorTargetIds = new RenderTargetIdentifier[8];

        static int lastColorIdsLength = 0;

        /// <summary>
        /// Save current targets, sfcpass can reuse these
        /// </summary>
        /// <param name="colorIds"></param>
        /// <param name="depthId"></param>
        public static void SaveTargets(RenderTargetIdentifier[] colorIds,RenderTargetIdentifier depthId)
        {
            if (LastColorTargetHandles == null)
                LastColorTargetHandles = new RTHandle[8];
            if (LastColorTargetIds == null)
                LastColorTargetIds = new RenderTargetIdentifier[8];

            int i = 0;
            for (; i < colorIds.Length; i++)
            {
                LastColorTargetIds[i] = colorIds[i];
                LastColorTargetHandles[i] = RTHandles.Alloc(colorIds[i]);
            }
            for (; i < 8; i++)
            {
                LastColorTargetIds[i] = 0;
                LastColorTargetHandles[i] = RTHandles.Alloc(0);
            }

            LastDepthTargetHandle = RTHandles.Alloc(depthId);
            lastColorIdsLength = colorIds.Length;
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
    }
}
