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

        public int GetDataStartId(int matPropId)
        => dataStartIds[matPropId];


        public BRGBatch(BatchRendererGroup brg, int numInstances, BatchMeshID meshId,BatchMaterialID matId,int brgBatchId)
        {
            this.brg = brg;
            this.numInstances = numInstances;
            this.meshId = meshId;
            this.matId = matId;
            this.brgBatchId = brgBatchId;
        }

        public void Dispose()
        {
            instanceBuffer.Dispose();
            instanceBuffer = null;
        }

        public void SetupGraphBuffer(int matPropfloatCount,string[] matPropNames,List<int> dataStartIdStrideList)
        {
            //--
            if (dataStartIdStrideList.Count == matPropNames.Length)
                dataStartIdStrideList.Insert(0, 0);

            dataStartIdStrides = dataStartIdStrideList.Select(x => x * numInstances).ToArray();

            //
            var count = matPropfloatCount * numInstances;
            Debug.Log($"count :{count}");
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw,count, 4);

            dataStartIds = new int[matPropNames.Length];

            var metadataList = new NativeArray<MetadataValue>(matPropNames.Length, Allocator.Temp);
            BRGTools.FillMetadatas(dataStartIdStrides, matPropNames, ref metadataList, ref startByteAddressDict, ref dataStartIds);

            batchId = brg.AddBatch(metadataList, instanceBuffer);
            metadataList.Dispose();
        }

        public void FillGraphBuffer(MeshRenderer[] mrs)
        {
            for (int i = 0; i < mrs.Length; i++)
            {
                var mr = mrs[i];
                var objectToWorld = mr.transform.localToWorldMatrix.ToFloat3x4();
                var worldToObject = mr.transform.worldToLocalMatrix.ToFloat3x4();
                var color = mr.sharedMaterial.color;

                FillData(objectToWorld.ToColumnArray(), i, 0);
                FillData(worldToObject.ToColumnArray(), i, 1);
                FillData(color.ToArray(), i, 2);
            }
        }

        public void FillData(float[] datas,int instanceId,int matPropId)
        {
            instanceBuffer.FillData(datas, instanceId* datas.Length, GetDataStartId(matPropId));
        }

        public void AddRenderers(IEnumerable<Renderer> renderers)
        {
            var instId = 0;
            foreach (var mr in renderers)
            {
                mr.enabled = false;

                var objectToWorld = mr.transform.localToWorldMatrix.ToFloat3x4();
                var worldToObject = mr.transform.worldToLocalMatrix.ToFloat3x4();
                Vector4 mainTex_ST = new float4(mr.sharedMaterial.mainTextureScale, mr.sharedMaterial.mainTextureOffset);
                var color = mr.sharedMaterial.color;


                FillData(objectToWorld.ToColumnArray(), instId, 0);
                FillData(worldToObject.ToColumnArray(), instId, 1);
                FillData(mainTex_ST.ToArray(), instId, 2);
                FillData(color.ToArray(), instId, 3);

                //========== block component
                var block = mr.gameObject.GetOrAddComponent<BRGBatchBlock>();
                block.brgBatch = this;
                block.instId = instId;

                instId++;
            }
        }

        public unsafe void DrawBatch(BatchCullingOutputDrawCommands* drawCmdPt,int visibleNumInstances=-1,int visibleOffset=0)
        {
            var visibleCount = visibleNumInstances < 0 ? numInstances : visibleNumInstances;
            BRGTools.FillBatchDrawCommand(drawCmdPt, brgBatchId, batchId, matId, meshId, visibleCount, visibleOffset);
        }
    }
}
