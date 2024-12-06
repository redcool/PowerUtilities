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
    public partial class DrawChildrenBRG : MonoBehaviour
    {
        public class DrawBatchInfo
        {
            public GraphicsBuffer buffer;
            public BatchMaterialID matId;
            public BatchMeshID meshId;
            public BatchID batchId;
            public int instanceCount;
        }

        [EditorButton(onClickCall = "RecordChildren")]
        public bool isRecord;

        public bool isIncludeInvisible = false;

        BatchRendererGroup brg;

        //IEnumerable<(GraphicsBuffer, IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer>)> drawInfos;
        // for rendering
        List<BRGBatch> batchList = new();


        void OnEnable()
        {
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            if (batchList.Count == 0)
            {
                var groupInfos = RegisterChildren();
                SetupGroupInfos(groupInfos);
            }
        }

        void RecordChildren()
        {
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            var groupInfos = RegisterChildren();
            SetupBRGGroupInfoList(groupInfos);

            batchList.Clear();
            batchList.AddRange(
                brgGroupInfoList.Select((brgGroupInfo, groupId) =>
                {
                    var brgBatch = new BRGBatch(brg, brgGroupInfo.instanceCount, brgGroupInfo.meshId, brgGroupInfo.matId, groupId);
                    brgBatch.SetupGraphBuffer(brgGroupInfo.floatsCount);

                    return brgBatch;
                })
            );
        }

        /// <summary>
        /// Same batch means : same (material,mesh)
        /// </summary>
        public IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> RegisterChildren()
        {
            var mrs = GetComponentsInChildren<MeshRenderer>(isIncludeInvisible);
            var groupInfos = from mr in mrs
                             let mf = mr.GetComponent<MeshFilter>()
                             where mf is not null
                             let sharedMesh = mf.sharedMesh
                             where sharedMesh is not null

                             group mr by (
                             mr.lightmapIndex,
                             brg.RegisterMesh(mf.sharedMesh),
                             brg.RegisterMaterial(mr.sharedMaterial)
                             ) into g
                             select g
                        ;
            return groupInfos;

        }

        public void SetupGroupInfos(IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> groupInfos)
        {
            var groupCount = groupInfos.Count();
            batchList.Clear();

            var groupId = 0;
            foreach (IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer> groupInfo in groupInfos)
            {
                var instCount = groupInfo.Count();

                // find material props
                var matPropNameList = new List<string>();
                var floatsCount = 0;

                var mat = brg.GetRegisteredMaterial(groupInfo.Key.matId);
                mat.shader.FindShaderPropNames_BRG(ref matPropNameList, ref floatsCount,null);

                var brgBatch = new BRGBatch(brg, instCount, groupInfo.Key.meshId, groupInfo.Key.matId, groupId);
                brgBatch.SetupGraphBuffer(floatsCount);

                batchList.Add(brgBatch);
                //----- add renderer

                var instId = 0;
                foreach (var mr in groupInfo)
                {
                    mr.enabled = false;
                    var objectToWorld = mr.transform.localToWorldMatrix.ToFloat3x4();
                    var worldToObject = mr.transform.worldToLocalMatrix.ToFloat3x4();
                    var color = mr.sharedMaterial.color;

                    brgBatch.FillData(objectToWorld.ToColumnArray(), instId, 0);
                    brgBatch.FillData(worldToObject.ToColumnArray(), instId, 1);
                    brgBatch.FillData(color.ToArray(), instId, 2);

                    //========== block component
                    var block = mr.gameObject.GetOrAddComponent<BRGBatchBlock>();
                    block.brgBatch = brgBatch;
                    block.instId = instId;

                    instId++;
                }

                groupId++;
            }

        }

        private void OnDisable()
        {
            foreach (var brgBatch in batchList)
            {
                brgBatch.Dispose();
            }
            if (brg != null)
                brg.Dispose();
        }

        public unsafe JobHandle OnPerformCulling(
            BatchRendererGroup rendererGroup,
            BatchCullingContext cullingContext,
            BatchCullingOutput cullingOutput,
            IntPtr userContext)
        {
            var numInstances = batchList.Sum(b => b.numInstances);
            BRGTools.SetupBatchDrawCommands(cullingOutput, batchList.Count, numInstances);

            foreach (var brgBatch in batchList)
            {
                brgBatch.DrawBatch(cullingOutput);
            }

            return new JobHandle();
        }
    }
}
#endif