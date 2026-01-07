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

        //public Material mat;
        //public Mesh mesh;
        /// <summary>
        /// {prop name , startByte = startId * sizeof(float)}
        /// </summary>
        public Dictionary<string, int> propNameStartFloatIdDict = new();

        // from outside
        BatchRendererGroup brg;
        public int numInstances;

        public int brgBatchId;

        // visible id list
        public List<int> visibleIdList = new();

        /// <summary>
        /// tools
        /// </summary>
        public BRGMaterialInfo brgMaterialInfo;


        public BRGBatch(BatchRendererGroup brg, int numInstances, BatchMeshID meshId,BatchMaterialID matId,int brgBatchId)
        {
            this.brg = brg;
            this.numInstances = numInstances;
            this.meshId = meshId;
            this.matId = matId;
            this.brgBatchId = brgBatchId;

            //mat = brg.GetRegisteredMaterial(matId);
            //mesh = brg.GetRegisteredMesh(meshId);
        }

        public void Dispose()
        {
            instanceBuffer.Dispose();
            instanceBuffer = null;
        }
        /// <summary>
        /// Setup this batch info
        /// 
        /// </summary>
        /// <param name="matPropfloatsCount"></param>
        /// <param name="matPropInfos"></param>
        public void Setup(int matPropfloatsCount,(string name,int floatCount)[] matPropInfos)
        {
            //var floatsCount = matPropInfos.Sum(info => info.floatCount);
            //
            var count = matPropfloatsCount * numInstances;
            Debug.Log($"all floats count :{count}");
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw,count, 4);

            var metadataList = new NativeArray<MetadataValue>(matPropInfos.Length, Allocator.Temp);
            BRGTools.SetupMetadatas(numInstances, matPropInfos, ref metadataList, propNameStartFloatIdDict);

            batchId = brg.AddBatch(metadataList, instanceBuffer);
            metadataList.Dispose();
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
                //FillMaterialDatas(this,instId, mr);
                onFillMaterailData?.Invoke(this, instId, mr);

                //========== block component
                var block = mr.gameObject.GetOrAddComponent<BRGBatchBlock>();
                block.brgBatch = this;
                block.instId = instId;
                block.brgMaterialInfo = brgMaterialInfo;

                instId++;
            }
        }

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
