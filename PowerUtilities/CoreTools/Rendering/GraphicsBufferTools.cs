using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public static class GraphicsBufferTools
    {
        public static int FLOAT_BYTES = 4;
        public static int FLOAT4_BYTES = 16;
        public static int MATRIX_BYTES = 16*4;
        public static int FLOAT3X4_BYTES = 12*4;

        /// <summary>
        /// {bufferName , GraphicsBuffer}
        /// </summary>
        public static Dictionary<string,GraphicsBuffer> bufferDict = new ();

        /// <summary>
        /// FloatStartId -> T startId
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="floatStartId"></param>
        /// <returns></returns>
        public static int GetStartId<T>(int floatStartId) where T : struct
        {
            return floatStartId / Marshal.SizeOf<T>();
        }

        public static void FillData(this GraphicsBuffer buffer, float[] floatDatas, int graphBufferStartId,int graphBufferStartIdOffset)
        {
            if (!buffer.IsValidSafe())
                return;

            //Debug.Log($"FillData startId : {graphBufferStartId} + {graphBufferStartIdOffset} ="+ (graphBufferStartId + graphBufferStartIdOffset));
            buffer.SetData(floatDatas, 0, graphBufferStartId + graphBufferStartIdOffset, floatDatas.Length);
        }
        
        /// <summary>
        /// Fill data to buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="floatDatas"></param>
        /// <param name="graphBufferStartId">instance start float id</param>
        /// <param name="graphBufferStartIdOffset">instance start float id offset</param>
        public static void FillInstanceData(this GraphicsBuffer buffer, float[] floatDatas, int instanceId, int graphBufferStartIdOffset)
        {
            if (!buffer.IsValidSafe())
                return;

            var graphBufferStartId = instanceId * floatDatas.Length;
            //Debug.Log($"FillData startId : {graphBufferStartId} + {graphBufferStartIdOffset} ="+ (graphBufferStartId + graphBufferStartIdOffset));
            buffer.SetData(floatDatas, 0, graphBufferStartId + graphBufferStartIdOffset, floatDatas.Length);
        }
        

        public static bool IsValidSafe(this GraphicsBuffer buffer)
        {
            return buffer != null && buffer.IsValid();
        }

        public static bool IsValidSafe(this GraphicsBuffer buffer,GraphicsBuffer.Target target,int count,int stride)
        {
            return IsValidSafe(buffer)
                && buffer.count == count 
                && buffer.stride == stride
                && buffer.target == target
                ;
        }

        public static void TryRelease(this GraphicsBuffer buffer)
        {
            if (buffer.IsValidSafe())
                buffer.Release();
        }
        /// <summary>
        /// create new when buffer is null or invalid
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="target"></param>
        /// <param name="count"></param>
        /// <param name="stride"></param>
        public static void TryCreateBuffer(ref GraphicsBuffer buffer,GraphicsBuffer.Target target,int count,int stride)
        {
            if (!buffer.IsValidSafe(target, count, stride)) {
                TryRelease(buffer);
                buffer = new GraphicsBuffer(target, count, stride);
            }
        }

        /// <summary>
        /// Get a global named (cached )buffer
        /// </summary>
        /// <param name="bufferName"></param>
        /// <param name="target"></param>
        /// <param name="count"></param>
        /// <param name="stride"></param>
        /// <returns></returns>
        public static GraphicsBuffer GetGlobalBuffer(string bufferName, GraphicsBuffer.Target target, int count, int stride)
        {
            bufferDict.TryGetValue(bufferName, out var buffer);

            if (!buffer.IsValidSafe(target,count, stride))
            {
                buffer.TryRelease();
                buffer = bufferDict[bufferName] = new GraphicsBuffer(target, count, stride);
                buffer.name = bufferName;
            }
            return buffer;

            //return DictionaryTools.Get(bufferDict, bufferName, bufferName => new GraphicsBuffer(target, count, stride));
        }
    }



}
