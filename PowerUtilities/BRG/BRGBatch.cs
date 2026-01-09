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
        /// {prop name , startByte = startId * sizeof(float)}
        /// </summary>
        public Dictionary<string, int> propNameStartFloatIdDict = new();

        public int numInstances;
        public int brgBatchId;

        // visible id list
        public List<int> visibleIdList = new();

        /// <summary>
        /// tools
        /// </summary>
        public BRGMaterialInfo brgMaterialInfo;

        // from outside
        BatchRendererGroup brg;
        /// <summary>
        /// all instances start index,cullingGroup use
        /// </summary>
        public int GlobalInstanceOffset => brgBatchId * numInstances;

        public BRGBatch(BatchRendererGroup brg, int numInstances, BatchMeshID meshId,BatchMaterialID matId,int brgBatchId)
        {
            this.brg = brg;
            this.numInstances = numInstances;
            this.meshId = meshId;
            this.matId = matId;
            this.brgBatchId = brgBatchId;

            // default no culling
            visibleIdList.AddRange(Enumerable.Range(0, numInstances));
        }

        public void Dispose()
        {
            instanceBuffer.Dispose();
            instanceBuffer = null;
        }
        /// <summary>
        /// Create instanceBuffer,MetadataValues, then addBatchto brg
        /// 
        /// when done ,propNameStartFloatIdDict can use get property start index(BRGTools.GetDataStartId
        /// </summary>
        /// <param name="matPropfloatsCount"></param>
        /// <param name="matPropInfos"></param>
        public void Setup((string name,int floatCount)[] matPropInfos)
        {
            batchId = BRGTools.AddBatch(brg, out instanceBuffer, numInstances, matPropInfos, propNameStartFloatIdDict);
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
