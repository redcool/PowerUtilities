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

namespace PowerUtilities
{
    public static class BRGTools
    {



        /// <summary>
        /// Get material variable's byte count
        /// </summary>
        /// <param name="varInfos"></param>
        /// <returns></returns>
        public static int GetByteCount(params (Type varType,int varCount)[] varInfos)
        {
            var count = 0;
            foreach (var info in varInfos)
            {
                count += Marshal.SizeOf(info.varType) * info.varCount;
            }
            return count;
        }

        public static BatchID AddBatch(ref BatchRendererGroup brg,NativeArray<MetadataValue> metadatas,GraphicsBuffer graphBuffer)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                return brg.AddBatch(metadatas, graphBuffer.bufferHandle, 0, (uint)BatchRendererGroup.GetConstantBufferMaxWindowSize());
            else
                return brg.AddBatch(metadatas, graphBuffer.bufferHandle);
        }

        /*
         * 
        var resultList = new List<Vector4>();
        var startByteAddressList = new List<int>();

        resultList.AddRange(zero.SelectMany(m => m.ToColumnVectors()));

        Debug.Log(resultList.Count);
        startByteAddressList.Add(resultList.Count * 16);
        resultList.AddRange(objectToWorld.SelectMany(m => m.ToColumnVectors()));

        Debug.Log(resultList.Count);
        startByteAddressList.Add(resultList.Count * 16);
        resultList.AddRange(worldToObject.SelectMany(m => m.ToColumnVectors()));

        Debug.Log(resultList.Count);
        startByteAddressList.Add(resultList.Count * 16);
        resultList.AddRange(colors);

        instanceBuffer.SetData(resultList);

        var matVariableNameList = new List<string>()
        {
            "unity_ObjectToWorld",
            "unity_WorldToObject",
            "_Color"
        };

        var metadatas = new NativeArray<MetadataValue>(3, Allocator.Temp);
        BRGTools.FillMetadatas(ref metadatas, matVariableNameList, startByteAddressList);
        m_BatchID = BRGTools.AddBatch(ref m_BRG,metadatas,instanceBuffer);


        public static void FillMetadatas(ref NativeArray<MetadataValue> metadatas, List<string> matVariableNameList, List<int> startByteAddressList)
        {
           if (metadatas == null)
                metadatas = new NativeArray<MetadataValue>(matVariableNameList.Count,Allocator.Temp);

            var q = matVariableNameList.Select((name, id) => new MetadataValue {
                NameID = Shader.PropertyToID(name), Value = (uint)(0x80000000 | startByteAddressList[id])
            });

           for (int i = 0; i < matVariableNameList.Count; i++)
            {
                var matName = matVariableNameList[i];
                var startByteAddress = startByteAddressList[i];

                metadatas[i] = new MetadataValue
                {
                    NameID = Shader.PropertyToID(matName),
                    Value = (uint)(0x80000000 | startByteAddress)
                };
            }
        }

         */

        /// <summary>
        /// Fill resultList,float4 base
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="metaDataList"></param>
        /// <param name="startByteAddressList"></param>
        /// <param name="datas"></param>
        /// <param name="matVarName"></param>
        public static void FillMatProperty(ref List<Vector4> resultList, ref NativeList<MetadataValue> metaDataList,ref List<int> startByteAddressList, IEnumerable<Vector4> datas, string matVarName)
        {
            //1  calc start byte address first
            // null dont need byteAddress
            if (!string.IsNullOrEmpty(matVarName))
            {
                var startByteAddress = resultList.Count * GraphicsBufferTools.VECTOR4_BYTES;

                startByteAddressList.Add(startByteAddress);

                metaDataList.Add(new MetadataValue
                {
                    NameID = Shader.PropertyToID(matVarName),
                    Value = (uint)(0x80000000 | startByteAddress)
                });
            }

            //====== add all Vector4 flatted
            resultList.AddRange(datas);
        }

        /// <summary>
        /// Fill resultList ,float base
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="metaDataList"></param>
        /// <param name="startByteAddressList"></param>
        /// <param name="datas"></param>
        /// <param name="matVarName"></param>
        public static void FillMatProperty(ref List<float> resultList, ref NativeList<MetadataValue> metaDataList, ref List<int> startByteAddressList, IEnumerable<float> datas, string matVarName)
        {
            //1  calc start byte address first
            // null dont need byteAddress
            if (!string.IsNullOrEmpty(matVarName))
            {
                var startByteAddress = resultList.Count * GraphicsBufferTools.FLOAT_BYTES;

                startByteAddressList.Add(startByteAddress);

                metaDataList.Add(new MetadataValue
                {
                    NameID = Shader.PropertyToID(matVarName),
                    Value = (uint)(0x80000000 | startByteAddress)
                });
            }

            //====== add all Vector4 flatted
            resultList.AddRange(datas);
        }
    }
}
