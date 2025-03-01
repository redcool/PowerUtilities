﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEditor;
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
        public static int GetByteCount(params (Type varType, int varCount)[] varInfos)
        {
            var count = 0;
            foreach (var info in varInfos)
            {
                count += Marshal.SizeOf(info.varType) * info.varCount;
            }
            return count;
        }

        public static int GetByteCount(int numInstance,params Type[] matTypes)
        {
            return matTypes.Select(type => Marshal.SizeOf(type)).Sum() * numInstance;
        }

        public static BatchID AddBatch(this BatchRendererGroup brg,NativeArray<MetadataValue> metadatas,GraphicsBuffer graphBuffer)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                return brg.AddBatch(metadatas, graphBuffer.bufferHandle, 0, (uint)BatchRendererGroup.GetConstantBufferMaxWindowSize());
            else
                return brg.AddBatch(metadatas, graphBuffer.bufferHandle);
        }

        public static unsafe void SetupBatchDrawCommands(BatchCullingOutput cullingOutput,int batchCount,int allVisibleInstanceCount)
        {
            int alignment = UnsafeUtility.AlignOf<long>();
            var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

            drawCmdPt->drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>() * batchCount, alignment, Allocator.TempJob);
            drawCmdPt->visibleInstances=(int*)UnsafeUtility.Malloc(sizeof(int) * allVisibleInstanceCount,alignment, Allocator.TempJob);
            drawCmdPt->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(), alignment, Allocator.TempJob);

            drawCmdPt->drawCommandCount = batchCount;

            drawCmdPt->visibleInstanceCount = allVisibleInstanceCount;
            for (int i = 0; i < allVisibleInstanceCount; ++i)
                drawCmdPt->visibleInstances[i] = i;

            drawCmdPt->drawCommandPickingInstanceIDs = null;

            drawCmdPt->instanceSortingPositions = null;
            drawCmdPt->instanceSortingPositionFloatCount = 0;

            // 
            drawCmdPt->drawRangeCount = 1;
            drawCmdPt->drawRanges[0].drawCommandsBegin = 0;
            drawCmdPt->drawRanges[0].drawCommandsCount = (uint)drawCmdPt->drawCommandCount;
            drawCmdPt->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };

        }

        public static unsafe void FillBatchDrawCommands(BatchCullingOutput cullingOutput, int cmdId, BatchID batchId, BatchMaterialID materialId, BatchMeshID meshId, int numInstances)
        {
            var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

            drawCmdPt->drawCommands[cmdId].batchID = batchId;
            drawCmdPt->drawCommands[cmdId].flags = 0;
            drawCmdPt->drawCommands[cmdId].materialID = materialId;
            drawCmdPt->drawCommands[cmdId].meshID = meshId;
            drawCmdPt->drawCommands[cmdId].sortingPosition = 0;
            drawCmdPt->drawCommands[cmdId].splitVisibilityMask = 0;
            drawCmdPt->drawCommands[cmdId].submeshIndex = 0;
            drawCmdPt->drawCommands[cmdId].visibleCount = (uint)numInstances;
            drawCmdPt->drawCommands[cmdId].visibleOffset = 0;
        }


        /// <summary>
        /// Find shader propNames,and need how many floats
        /// first add localToWorld,worldToLocal
        ///liek:
        //{
        //    "unity_ObjectToWorld", //12 floats
        //    "unity_WorldToObject", //12
        //    "_Color", //4
        //};
        //var floatsCount = 12 + 12 + 4;
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="floatsCount">total floats count</param>
        /// <param name="propFloatCountList">floats count per prop</param>
        /// <param name="isFindShaderProp">if not,only include {unity_ObjectToWorld,unity_WorldToObject}</param>
        /// <returns></returns>
        public static void FindShaderPropNames_BRG(this Shader shader, ref List<string> propNameList, ref int floatsCount, List<int> propFloatCountList, bool isFindShaderProp = true)
        {
            // add 2 matrix floatCount
            floatsCount = 12 + 12;
            // add 2 matrix
            propNameList.Clear();
            propNameList.Add("unity_ObjectToWorld");
            propNameList.Add("unity_WorldToObject");
            // add per prop floats
            if (propFloatCountList != null)
            {
                propFloatCountList.Add(12);
                propFloatCountList.Add(12);
            }

            if (isFindShaderProp)
                shader.FindShaderPropNames(ref propNameList, ref floatsCount, propFloatCountList);
        }


    }
}
