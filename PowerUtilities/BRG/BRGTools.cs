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
        /// Calculates the starting index of graphics bufffer for a material property based on the provided property name, element
        /// size, instance, and element count.
        /// </summary>
        /// <param name="propNameStartIdDict">A dictionary that maps material property names to their corresponding starting identifiers.</param>
        /// <param name="matPropName">The name of the material property for which to retrieve the starting identifier. Must exist as a key in the
        /// dictionary.</param>
        /// <param name="elementFloatCount">The number of float values that make up a single element of the property. Must be greater than zero. The
        /// default is 1.</param>
        /// <param name="instanceId">The zero-based index of the instance for which to calculate the starting identifier. The default is 0.</param>
        /// <param name="elementCount">The number of elements per instance. Must be greater than zero. The default is 1.</param>
        /// <returns>The calculated starting identifier for the specified material property and instance. -1 : no this prop</returns>
        public static int GetDataStartId(Dictionary<string, int> propNameStartIdDict, string matPropName, int elementFloatCount = 1, int instanceId = 0, int elementCount = 1)
        {
            if (!propNameStartIdDict.ContainsKey(matPropName))
                return -1;
            return propNameStartIdDict[matPropName] / elementFloatCount + instanceId * elementCount;
        }
        /// <summary>
        /// gles3 cbuffer 16k, 16k/matBytes = instanceCount
        /// otherwise return defaultInstanceCount
        /// </summary>
        /// <param name="defaultInstanceCount"></param>
        /// <param name="matBytes"></param>
        /// <returns></returns>
        public static int GetMaxInstanceCount(int defaultInstanceCount, int matBytes)
        {
            var windowSize = 0;
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
                windowSize = BatchRendererGroup.GetConstantBufferMaxWindowSize(); // gles3 16k
            var count = windowSize / matBytes;
            return count == 0 ? defaultInstanceCount : count;
        }

        /// <summary>
        /// Fill metadata (metadataList)<br/>
        /// propNameStartFloatIdDict {propName,floatCount StartId}<br/>
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
        /// <param name="instanceCount"></param>
        /// <param name="matPropInfos"></param>
        /// <param name="metadataList"></param>
        /// <param name="startIdDict">dict save mat property float index</param>
        /// <returns>floats count</returns>
        public static int SetupMetadatas(int instanceCount,
            (string matPropName, int matPropFloatCount)[] matPropInfos,
            ref NativeArray<MetadataValue> metadataList,
            Dictionary<string, int> startIdDict)
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
#if UNITY_6000_3_OR_NEWER
            drawCmdPt->drawCommandPickingEntityIds = null;
#else
            drawCmdPt->drawCommandPickingInstanceIDs = null;
#endif

            var drawRange0 = new BatchDrawRange
            {
#if UNITY_6000_3_OR_NEWER
                drawCommandsType = BatchDrawCommandType.Direct,
#endif
                drawCommandsBegin = 0,
                drawCommandsCount = (uint)batchCount,
                filterSettings = new BatchFilterSettings
                {
                    allDepthSorted = true,
                    renderingLayerMask = 0xffffffff,
                },
            };
            drawCmdPt->drawRanges[0] = drawRange0;
            drawCmdPt->drawRangeCount = 1;
        }


        public static unsafe BatchDrawCommand FillBatchDrawCommand(BatchCullingOutputDrawCommands* drawCmdPt, int cmdId, BatchID batchId, BatchMaterialID materialId, BatchMeshID meshId, int visibleCount,int visibleOffset=0)
        {
            var cmd = new BatchDrawCommand
            {
                batchID = batchId,
                flags = 0,
                materialID = materialId,
                meshID = meshId,
                sortingPosition = 0,
                splitVisibilityMask = 0,
                submeshIndex = 0,
                visibleCount = (uint)visibleCount,
                visibleOffset = (uint)visibleOffset
            };
            return drawCmdPt->drawCommands[cmdId] = cmd;
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
        /// Find shader propNames,floats <br/>
        /// first add localToWorld,worldToLocal,like<br/>
        ///{<br/>
        ///    "unity_ObjectToWorld", //12 floats<br/>
        ///    "unity_WorldToObject", //12<br/>
        ///    "_Color", //4<br/>
        ///};<br/>
        ///var floatsCount = 12 + 12 + 4;<br/>
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="floatsCount">total floats count</param>
        /// <param name="propFloatCountList">floats count per prop</param>
        /// <param name="isFindShaderProp">if not,only include {unity_ObjectToWorld,unity_WorldToObject}</param>
        /// <returns></returns>
        public static void FindShaderPropNames_BRG(this Shader shader, ref List<(string name,int floatCount)> propNameList, ref int floatsCount, bool isFindShaderProp = true,bool isSkipTexST=true,bool hasNormalMap=true)
        {
            //1 add 2 matrix floatCount
            floatsCount = 12;
            // add 2 matrix
            propNameList.Clear();
            propNameList.Add((unity_ObjectToWorld,12));

            if (hasNormalMap)
            {
                floatsCount += 12;
                propNameList.Add((unity_WorldToObject,12));
            }

            //2 add material properties continue
            if (isFindShaderProp)
                shader.FindShaderPropNames(ref propNameList, ref floatsCount,isSkipTexST);
        }

        /// <summary>
        /// Create instanceBuffer,setup MetadataValues, then addBatch to brg <br/>
        /// when done ,propNameStartFloatIdDict can use get property start index(BRGTools.GetDataStartId
        /// </summary>
        /// <param name="brg"></param>
        /// <param name="instanceBuffer"></param>
        /// <param name="numInstances"></param>
        /// <param name="matPropInfos"></param>
        /// <param name="propNameStartFloatIdDict"></param>
        /// <returns></returns>
        public static BatchID AddBatch(
            BatchRendererGroup brg,
            out GraphicsBuffer instanceBuffer,
            int numInstances,
            (string propName, int propFloatCount)[] matPropInfos,
            Dictionary<string, int> propNameStartFloatIdDict
        )
        {
            var metadataList = new NativeArray<MetadataValue>(matPropInfos.Length, Allocator.Temp);
            var allFloatCount = BRGTools.SetupMetadatas(numInstances, matPropInfos, ref metadataList, propNameStartFloatIdDict);

            //Debug.Log($"all instance mat float count :{allFloatCount} floats");
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, allFloatCount, sizeof(float));
            //Debug.Log("startIdDict : " + string.Join(',', propNameStartFloatIdDict.Values));//0,12*instCount,24*instCount

            var batchId = brg.AddBatch(metadataList, instanceBuffer);
            metadataList.Dispose();
            return batchId;
        }
    }
}
