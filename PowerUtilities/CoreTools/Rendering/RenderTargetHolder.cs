using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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
        /// Get array(with lengthAsKey length)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="placeHolderDict"> plance holder dictionary</param>
        /// <param name="lengthAsKey"> length : an array's length as key</param>
        /// <returns></returns>
        //public static T[] GetArrayFromPlaceHolderDict<T>(Dictionary<int, T[]> placeHolderDict,int lengthAsKey)
        //{
        //    placeHolderDict.TryGetValue(lengthAsKey, out var arr);
        //    if(arr == null)
        //        arr =placeHolderDict[lengthAsKey] = new T[lengthAsKey];
        //    return arr;
        //}


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
        /// base camera 's color target
        /// </summary>
        public static RTHandle BaseCameraLastColorTarget;
        static RenderTargetHolder()
        {
            RenderPipelineManager.endCameraRendering -= SaveBaseCameraInfo;
            RenderPipelineManager.endCameraRendering += SaveBaseCameraInfo;
        }

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

        public static void SaveBaseCameraInfo(ScriptableRenderContext context, Camera camera)
        {
            if (!camera.IsGameCamera())
                return;

            var addData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (!addData || addData.renderType != CameraRenderType.Base)
                return;

            var urpRenderer = (UniversalRenderer)addData.scriptableRenderer;
            RenderTargetHolder.BaseCameraLastColorTarget = urpRenderer.GetActiveCameraColorAttachment();
        }
    }
}
