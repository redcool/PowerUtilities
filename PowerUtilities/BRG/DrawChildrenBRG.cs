#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace PowerUtilities
{
    public class DrawChildrenBRG : MonoBehaviour
    {
        public class DrawBatchInfo
        {
            public GraphicsBuffer buffer;
            public BatchMaterialID matId;
            public BatchMeshID meshId;
            public BatchID batchId;
            public int instanceCount;
        }

        [EditorButton(onClickCall = "TestStart")]
        public bool isTest;

        public bool isIncludeInvisible = false;

        BatchRendererGroup brg;

        //IEnumerable<(GraphicsBuffer, IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer>)> drawInfos;
        List<DrawBatchInfo> batchList = new();

        void OnEnable()
        {
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            RegisterChildren();
        }
        /// <summary>
        /// Same batch means : same (material,mesh)
        /// </summary>
        private void RegisterChildren()
        {
            var mrs = GetComponentsInChildren<MeshRenderer>(isIncludeInvisible);
            var drawInfos = from mr in mrs
                        let mf = mr.GetComponent<MeshFilter>()
                        where mf is not null 
                        let sharedMesh = mf.sharedMesh
                        where sharedMesh is not null

                        group mr by (
                        mr.lightmapIndex,
                        brg.RegisterMesh(mf.sharedMesh),
                        brg.RegisterMaterial(mr.sharedMaterial)
                        ) into g
                        select (
                            new GraphicsBuffer(GraphicsBuffer.Target.Raw,g.Count(),4),
                            g)
                        ;
            
            var matNames = new[]
            {
                "unity_ObjectToWorld",
                "unity_WorldToObject",
                "_Color"
            };

            Dictionary<GraphicsBuffer,Dictionary<string, int>> allStartByteAddressDict = new();

            foreach ((GraphicsBuffer gbuffer,IGrouping<(int lightmapId,BatchMeshID meshId,BatchMaterialID matId),MeshRenderer> groupInfo) info in drawInfos)
            {
                var instanceCount = info.groupInfo.Count();
                var objectToWorlds = new float[instanceCount * 12];
                var worldToObjects = new float[instanceCount * 12];
                var colors = new float[instanceCount * 4];

                var dataStartIdStrides = new[]
                {
                    0,
                    instanceCount * 12,//objectToWorld,
                    instanceCount * 12,//worldToObject,
                    instanceCount * 4,//colors.Count
                };
                var dataStartIds = new int[matNames.Length];

                var floatsCount = dataStartIdStrides.Sum();

                var startByteAddressDict = DictionaryTools.Get(allStartByteAddressDict,info.gbuffer,(gbuffer)=> new Dictionary<string, int>());

                var metadatas = new NativeArray<MetadataValue>(matNames.Length, Allocator.Temp);
                GraphicsBufferTools.FillMetadatas(dataStartIdStrides, matNames, ref metadatas, ref startByteAddressDict,ref dataStartIds);

                //----- add renderer
                var id = 0;
                foreach (var mr in info.groupInfo)
                {
                    //Debug.Log(groupInfo);

                    var objectToWorld = mr.transform.localToWorldMatrix.ToFloat3x4().ToColumnArray();
                    var worldToObject = mr.transform.worldToLocalMatrix.ToFloat3x4().ToColumnArray();
                    var color = mr.sharedMaterial.color.ToArray();

                    info.gbuffer.FillData(objectToWorld, id * 12, dataStartIds[0]);
                    info.gbuffer.FillData(worldToObject, id * 12, dataStartIds[1]);
                    info.gbuffer.FillData(color, id * 4, dataStartIds[2]);

                    id++;
                }

                batchList.Add(
                    new DrawBatchInfo
                    {
                        batchId = BRGTools.AddBatch(brg, metadatas, info.gbuffer),
                        buffer = info.gbuffer,
                        instanceCount = instanceCount,
                        matId = info.groupInfo.Key.matId,
                        meshId = info.groupInfo.Key.meshId
                    }
                );
            }


        }

        private void OnDisable()
        {
            if(brg != null)
                brg.Dispose();
        }

        public unsafe JobHandle OnPerformCulling(
            BatchRendererGroup rendererGroup,
            BatchCullingContext cullingContext,
            BatchCullingOutput cullingOutput,
            IntPtr userContext)
        {
            foreach (var info in batchList)
            {
                BRGTools.DrawBatch(cullingOutput, info.batchId, info.matId, info.meshId, info.instanceCount);
            }

            return new JobHandle();
        }
    }
}
#endif