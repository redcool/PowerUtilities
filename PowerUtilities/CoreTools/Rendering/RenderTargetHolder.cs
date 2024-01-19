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
        public readonly static RenderTargetIdentifier[][] allColorIds = new RenderTargetIdentifier[][]
        {
            new RenderTargetIdentifier[0],
            new RenderTargetIdentifier[1],
            new RenderTargetIdentifier[2],
            new RenderTargetIdentifier[3],
            new RenderTargetIdentifier[4],
            new RenderTargetIdentifier[5],
            new RenderTargetIdentifier[6],
            new RenderTargetIdentifier[7],
            new RenderTargetIdentifier[8],
    };

        public readonly static RTHandle ZeroHandle = RTHandles.Alloc(0);

        public readonly static RTHandle[][] allHandles = new RTHandle[][]
        {
            new RTHandle[0],
            new RTHandle[1],
            new RTHandle[2],
            new RTHandle[3],
            new RTHandle[4],
            new RTHandle[5],
            new RTHandle[6],
            new RTHandle[7],
            new RTHandle[8],
        };

        /// <summary>
        /// keep these colorRTs(8),depthRT
        /// </summary>
        public static RTHandle LastDepthTargetHandle;

        public static RTHandle[] LastColorTargetHandles = new RTHandle[8];
        public static RenderTargetIdentifier[] LastColorTargetIds = new RenderTargetIdentifier[8];

        static int lastColorIdsLength = 0;

        public static Dictionary<RenderTargetIdentifier, RTHandle> rtIdHandleDict = new Dictionary<RenderTargetIdentifier, RTHandle>();
        /// <summary>
        /// Get cacheed rtHandle with rtid
        /// </summary>
        /// <param name="rtId"></param>
        /// <returns></returns>
        public static RTHandle GetRTHandle(RenderTargetIdentifier rtId)
        {
            if(!rtIdHandleDict.TryGetValue(rtId,out var handle))
            {
                handle = rtIdHandleDict[rtId] = RTHandles.Alloc(rtId);
            }
            return handle;
        }

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

            //var ids = allColorIds[colorIds.Length];
            //var handles = allHandles[colorIds.Length];

            //for (int i = 0; i < colorIds.Length; i++)
            //{
            //    ids[i] = colorIds[i];
            //    handles[i] = GetRTHandle(colorIds[i]);
            //}


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
