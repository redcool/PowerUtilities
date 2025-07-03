using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public static class GraphicsBufferTools
    {
        public static int FLOAT_BYTES = 4;
        public static int FLOAT4_BYTES = 16;
        public static int MATRIX_BYTES = 64;
        public static int FLOAT3X4_BYTES = 12;

        /// <summary>
        /// {bufferName , GraphicsBuffer}
        /// </summary>
        public static Dictionary<string,GraphicsBuffer> bufferDict = new ();

        /// <summary>
        /// Set continuous Data block and update graphBufferStartId
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="datas"></param>
        /// <param name="graphBufferStartId"></param>
        public static void FillDataBlock(this GraphicsBuffer buffer, float[] datas, ref int graphBufferStartId)
        {
            buffer.SetData(datas, 0, graphBufferStartId, datas.Length);
            graphBufferStartId += datas.Length;
        }

        public static void FillData(this GraphicsBuffer buffer, float[] datas, int graphBufferStartId,int graphBufferStartIdOffset)
        {
            if (!buffer.IsValid())
                return;

            //Debug.Log($"FillData startId : {graphBufferStartId} + {graphBufferStartIdOffset} ="+ (graphBufferStartId + graphBufferStartIdOffset));
            buffer.SetData(datas, 0, graphBufferStartId + graphBufferStartIdOffset, datas.Length);
        }

        public static void FillMetadatas(int[] dataStartIdOffsets,string[] matPropNames, 
            ref NativeArray<MetadataValue> metadataList, ref Dictionary<string, int> startByteAddressDict,ref int[] dataStartIds)
        {
            for (int i = 0; i<matPropNames.Length; i++)
            {
                var startId = dataStartIdOffsets.Take(i+1).Sum();
                //Debug.Log("FillMetadatas,startId : " + startId);

                var matPropName = matPropNames[i];

                var startByteAddr = startId * FLOAT_BYTES;
                // metadatas
                metadataList[i] = new MetadataValue
                {
                    NameID = Shader.PropertyToID(matPropName),
                    Value = (uint)(0x80000000 | startByteAddr)
                };

                startByteAddressDict.Add(matPropName, startByteAddr);
                dataStartIds[i] = startId;
            }
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
