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

            Debug.Log($"FillData startId : {graphBufferStartId} + {graphBufferStartIdOffset} ="+ (graphBufferStartId + graphBufferStartIdOffset));
            buffer.SetData(datas, 0, graphBufferStartId + graphBufferStartIdOffset, datas.Length);
        }

        public static void FillMetadatas(int[] dataStartIdOffsets,string[] matPropNames, 
            ref NativeArray<MetadataValue> metadataList, ref Dictionary<string, int> startByteAddressDict,ref int[] dataStartIds)
        {
            for (int i = 0; i<matPropNames.Length; i++)
            {
                var startId = dataStartIdOffsets.Take(i+1).Sum();
                Debug.Log("FillMetadatas,startId : " + startId);

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

    }



}
