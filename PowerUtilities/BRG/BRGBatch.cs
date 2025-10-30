using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using Unity.Collections;
using System.Globalization;
using Unity.Mathematics;

namespace PowerUtilities
{
    /// <summary>
    /// Control a BatchRenderGroup
    /// </summary>
    public class BRGBatch
    {
        public GraphicsBuffer instanceBuffer;
        public BatchID batchId;
        public BatchMeshID meshId;
        public BatchMaterialID matId;

        public Material mat;
        public Mesh mesh;
        /// <summary>
        /// {prop name , start float address}
        /// </summary>
        public Dictionary<string, int> startByteAddressDict = new();

        // from outside
        BatchRendererGroup brg;
        public int numInstances;

        public int brgBatchId;

        // need fill
        int[] dataStartIdStrides;
        int[] dataStartIds;

        // visible id list
        public List<int> visibleIdList;

        /// <summary>
        /// tools
        /// </summary>
        public ShaderCBufferVar shaderCBufferVar;

        public int GetDataStartId(int matPropId)
        {
            if (matPropId >= dataStartIds.Length)
                throw new Exception($"matPropId({matPropId}) > dataStartIds length ({dataStartIds.Length})");
            return dataStartIds[matPropId];
        }


        public BRGBatch(BatchRendererGroup brg, int numInstances, BatchMeshID meshId,BatchMaterialID matId,int brgBatchId)
        {
            this.brg = brg;
            this.numInstances = numInstances;
            this.meshId = meshId;
            this.matId = matId;
            this.brgBatchId = brgBatchId;

            mat = brg.GetRegisteredMaterial(matId);
            mesh = brg.GetRegisteredMesh(meshId);
        }

        public void Dispose()
        {
            instanceBuffer.Dispose();
            instanceBuffer = null;
        }
        /// <summary>
        /// Setup this batch info 
        /// (dataStartIdStrides,instanceBuffer,dataStartIds,batchId)
        /// </summary>
        /// <param name="matPropfloatCount"></param>
        /// <param name="matPropNames"></param>
        /// <param name="dataStartIdStrideList"></param>
        public void Setup(int matPropfloatCount,string[] matPropNames,List<int> dataStartIdStrideList)
        {
            //--
            if (dataStartIdStrideList.Count == matPropNames.Length)
                dataStartIdStrideList.Insert(0, 0);

            dataStartIdStrides = dataStartIdStrideList.Select(x => x * numInstances).ToArray();

            //
            var count = matPropfloatCount * numInstances;
            Debug.Log($"all floats count :{count}");
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw,count, 4);

            dataStartIds = new int[matPropNames.Length];

            var metadataList = new NativeArray<MetadataValue>(matPropNames.Length, Allocator.Temp);
            BRGTools.FillMetadatas(dataStartIdStrides, matPropNames, ref metadataList, ref startByteAddressDict, ref dataStartIds);

            batchId = brg.AddBatch(metadataList, instanceBuffer);
            metadataList.Dispose();
        }

        /// <summary>
        /// Fill dato into instanceBuffer(RawByteBuffer)
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="instanceId"></param>
        /// <param name="matPropId"></param>
        public void FillData(float[] datas,int instanceId,int matPropId)
        {
            instanceBuffer.FillData(datas, instanceId* datas.Length, GetDataStartId(matPropId));
        }
        /// <summary>
        /// Add renderers material data to instanceBuffer
        /// 1 Fill material data
        /// 2 Setup BrgBatchBlock to renderer.gameObject
        /// </summary>
        /// <param name="renderers"></param>
        public void FillMaterialDataAndSetupBatchBlock(IEnumerable<Renderer> renderers,Action<BRGBatch,int ,Renderer> onFillMaterailData)
        {
            var instId = 0;
            foreach (var mr in renderers)
            {
                mr.enabled = false;

                //FillMaterialDatas(this,instId, mr);
                onFillMaterailData?.Invoke(this, instId, mr);

                //========== block component
                var block = mr.gameObject.GetOrAddComponent<BRGBatchBlock>();
                block.brgBatch = this;
                block.instId = instId;
                block.shaderCBufferVar = shaderCBufferVar;

                instId++;
            }
        }


        /// <summary>
        /// Fill this datas into graphBuffer
        /// {
        ///     objectToWorld,
        ///     worldToObject,
        ///     mainTex_ST,
        ///     color
        /// }
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="mr"></param>
        //public static void DefaultFillMaterialDatas(BRGBatch brgBatch, int instId, Renderer mr)
        //{
        //    var objectToWorld = mr.transform.localToWorldMatrix.ToFloat3x4();
        //    var worldToObject = mr.transform.worldToLocalMatrix.ToFloat3x4();
        //    Vector4 mainTex_ST = new float4(mr.sharedMaterial.mainTextureScale, mr.sharedMaterial.mainTextureOffset);
        //    var color = mr.sharedMaterial.color;

        //    brgBatch.FillData(objectToWorld.ToColumnArray(), instId, 0);
        //    brgBatch.FillData(worldToObject.ToColumnArray(), instId, 1);
        //    brgBatch.FillData(mainTex_ST.ToArray(), instId, 2);
        //    brgBatch.FillData(color.ToArray(), instId, 3);
        //}

        /// <summary>
        /// setup drawCmdPt prepare batch draw
        /// </summary>
        /// <param name="drawCmdPt"></param>
        /// <param name="visibleNumInstances"></param>
        /// <param name="visibleOffset"></param>
        public unsafe void DrawBatch(BatchCullingOutputDrawCommands* drawCmdPt,int visibleNumInstances=-1,int visibleOffset=0)
        {
            var visibleCount = visibleNumInstances < 0 ? numInstances : visibleNumInstances;
            BRGTools.FillBatchDrawCommand(drawCmdPt, brgBatchId, batchId, matId, meshId, visibleCount, visibleOffset);
        }
    }
}
