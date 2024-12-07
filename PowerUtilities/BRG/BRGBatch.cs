using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using Unity.Collections;
using System.Globalization;

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

        public Dictionary<string, int> startByteAddressDict = new();

        // from outside
        BatchRendererGroup brg;
        public int numInstances;

        public int brgBatchId;

        // need fill
        public string[] matPropNames = new[]
            {
            "unity_ObjectToWorld",
            "unity_WorldToObject",
            "_Color"
        };

        readonly int[] defaultDataStartIdStrides = new[]{
            0,//
            12, // objectToWorld
            12, //worldToObject
            4 //color
        };

        // need fill
        int[] dataStartIdStrides;
        int[] dataStartIds;

        public int GetDataStartId(int matPropId)
        => dataStartIds[matPropId];

        public int[] DataStartIdStrides
        {
            set { dataStartIdStrides = value; }
            get
            {
                if (dataStartIdStrides == null)
                    dataStartIdStrides = defaultDataStartIdStrides.Select(x => x * numInstances).ToArray();
                return dataStartIdStrides;
            }
        }

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
        }

        public void SetupGraphBuffer(int matPropfloatCount)
        {
            var count = matPropfloatCount * numInstances;
            Debug.Log($"count :{count}");
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw,count, 4);

            dataStartIds = new int[matPropNames.Length];

            var metadataList = new NativeArray<MetadataValue>(matPropNames.Length, Allocator.Temp);
            GraphicsBufferTools.FillMetadatas(DataStartIdStrides, matPropNames, ref metadataList, ref startByteAddressDict, ref dataStartIds);

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

        public void DrawBatch(BatchCullingOutput output)
        {
            BRGTools.FillBatchDrawCommands(output, brgBatchId, batchId, matId, meshId, numInstances);
        }
    }
}
