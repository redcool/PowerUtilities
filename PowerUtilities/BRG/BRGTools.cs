using System;
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
        public const string unity_ObjectToWorld = nameof(unity_ObjectToWorld);
        public const string unity_WorldToObject = nameof(unity_WorldToObject);
        public const string _Color = nameof(_Color);

        public static readonly Matrix4x4[] ZERO_MATRICES = new []{Matrix4x4.zero};
        private static readonly int FLOAT_BYTES = 4;


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


        /// <summary>
        /// Fill metadata (metadataList)<br/>
        /// startIdDict {propName,floatCount StartId}<br/>
        /// 
        /// 2 instance data layout,like:<br/>
        /// <br/>
        /// ------------ startByteAddrz objectToWorld = 0 <br/>
        /// inst 0 objectToWorld<br/>
        /// inst 1 objectToWorld<br/>
        /// ------------ startByteAddr worldToObject = 0+48*2<br/>
        /// inst 0 worldToObject<br/>
        /// inst 1 worldToObject<br/>
        /// ------------ startByteAddr color = 96 + 48*2<br/>
        /// inst 0 color<br/>
        /// inst 1 color<br/>
        /// 
        /// </summary>
        /// <param name="metadataList"></param>
        /// <param name="startIdDict"></param>
        public static int SetupMetadatas(int instanceCount, (string matPropName, int matPropFloatCount)[] matPropInfos,
            ref NativeArray<MetadataValue> metadataList, Dictionary<string, int> startIdDict)
        {
            var floatCountStartId = 0; // float count offset
            for (int i = 0; i < matPropInfos.Length; i++)
            {
                var matPropInfo = matPropInfos[i];

                var startByteAddr = floatCountStartId * FLOAT_BYTES;
                // metadatas
                metadataList[i] = new MetadataValue
                {
                    NameID = Shader.PropertyToID(matPropInfo.matPropName),
                    Value = 0x80000000 | (uint)startByteAddr // 0x80000000 is a flag for BRG, 0x1 : instance prop, 0: shared prop
                };

                startIdDict.Add(matPropInfo.matPropName, floatCountStartId);

                //next property float count stride
                floatCountStartId += matPropInfo.matPropFloatCount * instanceCount;
            }
            return floatCountStartId;
        }

        public static unsafe void SetupBatchDrawCommands(BatchCullingOutputDrawCommands* drawCmdPt, int batchCount,int allVisibleInstanceCount)
        {
            int alignment = UnsafeUtility.AlignOf<long>();

            drawCmdPt->drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>() * batchCount, alignment, Allocator.TempJob);
            drawCmdPt->visibleInstances=(int*)UnsafeUtility.Malloc(sizeof(int) * allVisibleInstanceCount,alignment, Allocator.TempJob);
            drawCmdPt->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(), alignment, Allocator.TempJob);

            drawCmdPt->drawCommandCount = batchCount;

            drawCmdPt->visibleInstanceCount = allVisibleInstanceCount;
            for (int i = 0; i < allVisibleInstanceCount; ++i)
                drawCmdPt->visibleInstances[i] = i;


            drawCmdPt->instanceSortingPositions = null;
            drawCmdPt->instanceSortingPositionFloatCount = 0;

            // 
#if UNITY_6000_2_OR_NEWER
            drawCmdPt->drawRanges[0].drawCommandsType = BatchDrawCommandType.Direct;
            drawCmdPt->drawCommandPickingEntityIds = null;
#else
            drawCmdPt->drawCommandPickingInstanceIDs = null;
#endif

            drawCmdPt->drawRanges[0].drawCommandsBegin = 0;
            drawCmdPt->drawRanges[0].drawCommandsCount = (uint)drawCmdPt->drawCommandCount;
            drawCmdPt->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };

            drawCmdPt->drawRangeCount = 1;
        }


        public static unsafe BatchDrawCommand FillBatchDrawCommand(BatchCullingOutputDrawCommands* drawCmdPt, int cmdId, BatchID batchId, BatchMaterialID materialId, BatchMeshID meshId, int visibleCount,int visibleOffset=0)
        {
            drawCmdPt->drawCommands[cmdId].batchID = batchId;
            drawCmdPt->drawCommands[cmdId].flags = 0;
            drawCmdPt->drawCommands[cmdId].materialID = materialId;
            drawCmdPt->drawCommands[cmdId].meshID = meshId;
            drawCmdPt->drawCommands[cmdId].sortingPosition = 0;
            drawCmdPt->drawCommands[cmdId].splitVisibilityMask = 0;
            drawCmdPt->drawCommands[cmdId].submeshIndex = 0;
            drawCmdPt->drawCommands[cmdId].visibleCount = (uint)visibleCount;
            drawCmdPt->drawCommands[cmdId].visibleOffset = (uint)visibleOffset;

            return drawCmdPt->drawCommands[cmdId];
        }

        /// <summary>
        /// Setup all visiable instance id list for BRG
        /// </summary>
        /// <param name="drawCmdPt"></param>
        /// <param name="allVisibleIdList"></param>
        public static unsafe void SetupBatchAllVisible(BatchCullingOutputDrawCommands* drawCmdPt, List<int> allVisibleIdList)
        {
            var allVisibleInstanceCount = allVisibleIdList.Count;
            drawCmdPt->visibleInstanceCount = allVisibleInstanceCount;
            for (int i = 0; i < allVisibleInstanceCount; ++i)
                drawCmdPt->visibleInstances[i] = allVisibleIdList[i];
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
        public static void FindShaderPropNames_BRG(this Shader shader, ref List<(string name,int floatCount)> propNameList, ref int floatsCount, bool isFindShaderProp = true)
        {
            //1 add 2 matrix floatCount
            floatsCount = 12 + 12;
            // add 2 matrix
            propNameList.Clear();
            propNameList.Add((unity_ObjectToWorld,12));
            propNameList.Add((unity_WorldToObject,12));

            //2 add material properties continue
            if (isFindShaderProp)
                shader.FindShaderPropNames(ref propNameList, ref floatsCount);
        }

        public static BatchID AddBatch(
            BatchRendererGroup brg,
            ref GraphicsBuffer instanceBuffer,
            int numInstances,
            (string propName, int propFloatCount)[] matPropInfos,
            Dictionary<string, int> startIdDict
        )
        {
            var metadataList = new NativeArray<MetadataValue>(matPropInfos.Length, Allocator.Temp);
            var allFloatCount = BRGTools.SetupMetadatas(numInstances, matPropInfos, ref metadataList, startIdDict);

            Debug.Log($"all instance mat float count :{allFloatCount} floats");
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, allFloatCount, sizeof(float));
            Debug.Log("startIdDict : " + string.Join(',', startIdDict.Values));//0,12*instCount,24*instCount

            var batchId = brg.AddBatch(metadataList, instanceBuffer);
            metadataList.Dispose();
            return batchId;

            //instanceBuffer.SetData(objectToWorlds, 0, GetDataStartId("unity_ObjectToWorld", 12), objectToWorlds.Count);
            //instanceBuffer.SetData(worldToObjects, 0, GetDataStartId("unity_WorldToObject", 12), numInstances);
            //instanceBuffer.SetData(colors, 0, GetDataStartId("_Color", 4), numInstances);
        }
    }
}
