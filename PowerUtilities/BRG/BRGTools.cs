using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace PowerUtilities
{
    public static class BRGTools
    {

        public static readonly Matrix4x4[] ZERO_MATRICES = new []{Matrix4x4.zero};


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
        /// Fill resultList ,float base, fill resultList only when matVarName is empty
        /// </summary>
        /// <param name="resultList"></param>
        /// <param name="metaDataList"></param>
        /// <param name="startByteAddressList"></param>
        /// <param name="datas"></param>
        /// <param name="matVarName"></param>
        public static void FillMatProperty(ref List<float> resultList, ref NativeList<MetadataValue> metaDataList, ref Dictionary<string,int> startByteAddressDict, IEnumerable<float> datas, string matVarName)
        {
            //1  calc start byte address first
            // null dont need byteAddress
            if (!string.IsNullOrEmpty(matVarName))
            {
                var startByteAddress = resultList.Count * GraphicsBufferTools.FLOAT_BYTES;

                startByteAddressDict[matVarName] = startByteAddress;

                metaDataList.Add(new MetadataValue
                {
                    NameID = Shader.PropertyToID(matVarName),
                    Value = (uint)(0x80000000 | startByteAddress)
                });
            }

            //====== add all Vector4 flatted
            resultList.AddRange(datas);
        }

        public static void FillMatPropZeroMatrices(ref List<float> resultList)
        {
            resultList.AddRange(ZERO_MATRICES.SelectMany(m => m.ToColumnArray()));
        }

        public static void FillMatProperties(ref List<float> resultList, ref NativeList<MetadataValue> metaDataList, ref Dictionary<string,int> startByteAddressDict, IList<(IEnumerable<float> datas, string matVarName)> dataInfos,bool isFillZeroMatrix=true)
        {
            resultList = resultList ?? new List<float>();
            if (!metaDataList.IsCreated)
                metaDataList = new NativeList<MetadataValue>(dataInfos.Count, AllocatorManager.Temp);
            startByteAddressDict = startByteAddressDict ?? new Dictionary<string, int>();

            // fill zero matrix
            if(isFillZeroMatrix)
                FillMatPropZeroMatrices(ref resultList);

            // fill others material properties
            foreach (var dataInfo in dataInfos)
            {
                FillMatProperty(ref resultList, ref metaDataList, ref startByteAddressDict, dataInfo.datas, dataInfo.matVarName);
            }
        }


        public static unsafe void DrawBatch(BatchCullingOutput cullingOutput,BatchID batchId,BatchMaterialID materialId,BatchMeshID meshId,int numInstances)
        {
            // UnsafeUtility.Malloc() requires an alignment, so use the largest integer type's alignment
            // which is a reasonable default.
            int alignment = UnsafeUtility.AlignOf<long>();

            // Acquire a pointer to the BatchCullingOutputDrawCommands struct so you can easily
            // modify it directly.
            var drawCommands = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

            // Allocate memory for the output arrays. In a more complicated implementation, you would calculate
            // the amount of memory to allocate dynamically based on what is visible.
            // This example assumes that all of the instances are visible and thus allocates
            // memory for each of them. The necessary allocations are as follows:
            // - a single draw command (which draws kNumInstances instances)
            // - a single draw range (which covers our single draw command)
            // - kNumInstances visible instance indices.
            // You must always allocate the arrays using Allocator.TempJob.
            drawCommands->drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>(), alignment, Allocator.TempJob);
            drawCommands->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(), alignment, Allocator.TempJob);
            drawCommands->visibleInstances = (int*)UnsafeUtility.Malloc(numInstances * sizeof(int), alignment, Allocator.TempJob);
            drawCommands->drawCommandPickingInstanceIDs = null;

            drawCommands->drawCommandCount = 1;
            drawCommands->drawRangeCount = 1;
            drawCommands->visibleInstanceCount = numInstances;

            // This example doens't use depth sorting, so it leaves instanceSortingPositions as null.
            drawCommands->instanceSortingPositions = null;
            drawCommands->instanceSortingPositionFloatCount = 0;

            // Configure the single draw command to draw kNumInstances instances
            // starting from offset 0 in the array, using the batch, material and mesh
            // IDs registered in the Start() method. It doesn't set any special flags.
            drawCommands->drawCommands[0].visibleOffset = 0;
            drawCommands->drawCommands[0].visibleCount = (uint)numInstances;
            drawCommands->drawCommands[0].batchID = batchId;
            drawCommands->drawCommands[0].materialID = materialId;
            drawCommands->drawCommands[0].meshID = meshId;
            drawCommands->drawCommands[0].submeshIndex = 0;
            drawCommands->drawCommands[0].splitVisibilityMask = 0xff;
            drawCommands->drawCommands[0].flags = 0;
            drawCommands->drawCommands[0].sortingPosition = 0;

            // Configure the single draw range to cover the single draw command which
            // is at offset 0.
            drawCommands->drawRanges[0].drawCommandsBegin = 0;
            drawCommands->drawRanges[0].drawCommandsCount = 1;

            // This example doesn't care about shadows or motion vectors, so it leaves everything
            // at the default zero values, except the renderingLayerMask which it sets to all ones
            // so Unity renders the instances regardless of mask settings.
            drawCommands->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };

            // Finally, write the actual visible instance indices to the array. In a more complicated
            // implementation, this output would depend on what is visible, but this example
            // assumes that everything is visible.
            for (int i = 0; i < numInstances; ++i)
                drawCommands->visibleInstances[i] = i;
        }

    }
}
