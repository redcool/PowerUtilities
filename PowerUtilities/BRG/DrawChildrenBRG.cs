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
    [ExecuteInEditMode]
    public partial class DrawChildrenBRG : MonoBehaviour
    {

        [EditorButton(onClickCall = "RecordChildren")]
        public bool isRecord;

        [Tooltip("Find meshRenderer from children include invisible")]
        public bool isIncludeInvisible = false;

        BatchRendererGroup brg;

        //IEnumerable<(GraphicsBuffer, IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer>)> drawInfos;
        // for rendering
        List<BRGBatch> batchList = new();


        void OnEnable()
        {
            CheckBRGMaterialInfoListSO();
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            if (brgGroupInfoList.Count == 0)
            {
                var groupInfos = GetChildrenGroups();
                FillBatchListWithGroupInfos(groupInfos);
            }
            else
            {
                FillBatchListWithBrgGroupInfoList();
            }
            SetupCommonCullingGroup();
        }

        private void OnDisable()
        {
            foreach (var brgBatch in batchList)
            {
                brgBatch.Dispose();
            }
            if (brg != null)
            {
                brg.Dispose();
                brg = null;
            }
        }
        void RecordChildren()
        {
            CheckBRGMaterialInfoListSO();
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            var groupInfos = GetChildrenGroups();
            SetupBRGGroupInfoList(groupInfos);

            FillBatchListWithBrgGroupInfoList();

            SetupCommonCullingGroup();
        }

        private void CheckBRGMaterialInfoListSO()
        {
            if (!brgMaterialInfoListSO || brgMaterialInfoListSO.brgMaterialInfoList.Count == 0)
            {
                throw new Exception($"{nameof(brgMaterialInfoListSO)} need config first");
            }
        }


        /// <summary>
        /// Group children by (lightmapIndex,mesh,material)
        /// Same batch means : same (material,mesh)
        /// </summary>
        public IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> GetChildrenGroups()
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
        /// <summary>
        /// Fill batchList from groupInfo grouped by { (lightmapIndex,batchMeshId,batchMaterialId), renderers}
        /// </summary>
        /// <param name="groupInfos"></param>
        public void FillBatchListWithGroupInfos(IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> groupInfos)
        {
            var groupCount = groupInfos.Count();
            batchList.Clear();

            var groupId = 0;
            var instIdStart = 0;
            foreach (IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer> groupInfo in groupInfos)
            {
                var instCount = groupInfo.Count();

                // find material props
                var matPropInfoList = new List<(string name,int floatCount)>();
                var floatsCount = 0;

                var mat = brg.GetRegisteredMaterial(groupInfo.Key.matId);
                mat.shader.FindShaderPropNames_BRG(ref matPropInfoList, ref floatsCount);
                
                var brgBatch = new BRGBatch(brg, instCount, groupInfo.Key.meshId, groupInfo.Key.matId, groupId);
                //setup shaderCBufferVar
                var brgMaterialInfo = brgMaterialInfoListSO.brgMaterialInfoList.Find(bufferVar => bufferVar.shader == mat.shader);
                brgBatch.brgMaterialInfo = brgMaterialInfo;

                brgBatch.Setup(floatsCount, matPropInfoList.ToArray());
                brgBatch.FillMaterialDataAndSetupBatchBlock(groupInfo, brgMaterialInfo.FillMaterialDatas);


                batchList.Add(brgBatch);
                groupId++;
                instIdStart += instCount;
            }

        }
        /// <summary>
        /// Fill batchList from brgGroupInfo(saved)
        /// </summary>
        private void FillBatchListWithBrgGroupInfoList()
        {
            batchList.Clear();
            batchList.AddRange(
                brgGroupInfoList.Select((brgGroupInfo, groupId) =>
                {
                    var meshId = brg.RegisterMesh(brgGroupInfo.mesh);
                    var matId = brg.RegisterMaterial(brgGroupInfo.mat);

                    var brgBatch = new BRGBatch(brg, brgGroupInfo.instanceCount, meshId, matId, groupId);
                    brgBatch.brgMaterialInfo = brgGroupInfo.brgMaterialInfo;

                    brgBatch.Setup(brgGroupInfo.floatsCount,
                        brgGroupInfo.matGroupList.Select(matInfo => (matInfo.propName,matInfo.floatsCount)).ToArray()
                        );

                    brgBatch.FillMaterialDataAndSetupBatchBlock(brgGroupInfo.rendererList, brgGroupInfo.brgMaterialInfo.FillMaterialDatas);
                    brgBatch.visibleIdList = brgGroupInfo.visibleIdList;

                    return brgBatch;
                })
            );

        }

        public void SetupCommonCullingGroup()
        {
            if (!cullingGroupControl)
                return;
            for (int i = 0; i < brgGroupInfoList.Count; i++)
            {
                var isClear = i == 0;
                var isSetBoundingSpheres = i == brgGroupInfoList.Count - 1;
                var brgGroupInfo = brgGroupInfoList[i];
                cullingGroupControl.SetupCullingInfos(brgGroupInfo.rendererList, isClear, isSetBoundingSpheres,i);
            }

            cullingGroupControl.OnStateChanged -= CullingGroupControl_OnStateChanged;
            cullingGroupControl.OnStateChanged += CullingGroupControl_OnStateChanged;

            cullingGroupControl.SetupCullingInfosVisible();
        }

        private void CullingGroupControl_OnStateChanged(CommomCullingInfo info)
        {
            if (brgGroupInfoList.Count <= info.batchGroupId)
                return;

            var groupInfo = brgGroupInfoList[info.batchGroupId];
            // remove target id first
            groupInfo.visibleIdList.Remove(info.visibleId);

            if (info.IsVisible)
                groupInfo.visibleIdList.Add(info.visibleId);
            else
                groupInfo.visibleIdList.Remove(info.visibleId);

            //Debug.Log($"visible changed: groupid: {info.batchGroupId}, visibleId:{info.visibleId}");
        }

        public unsafe JobHandle OnPerformCulling(
            BatchRendererGroup rendererGroup,
            BatchCullingContext cullingContext,
            BatchCullingOutput cullingOutput,
            IntPtr userContext)
        {
            var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

            //Test1Batch(drawCmdPt);

            DrawBatchList(drawCmdPt);

            return new JobHandle();
        }

        private unsafe void DrawBatchList(BatchCullingOutputDrawCommands* drawCmdPt)
        {
            var allVisibleList = batchList.SelectMany(b => b.visibleIdList).ToList();
            var allVisibleCount = allVisibleList.Count();

            BRGTools.SetupBatchDrawCommands(drawCmdPt, batchList.Count, allVisibleCount);
            BRGTools.SetupBatchAllVisible(drawCmdPt, allVisibleList);

            var visibleOffset = 0;
            for (int i = 0; i < batchList.Count; i++)
            {
                var brgBatch = batchList[i];
                brgBatch.DrawBatch(drawCmdPt, brgBatch.visibleIdList.Count, visibleOffset);
                //brgBatch.DrawBatch(drawCmdPt);

                visibleOffset += brgBatch.visibleIdList.Count;
            }
        }

        private unsafe void Test1Batch(BatchCullingOutputDrawCommands* drawCmdPt)
        {
            // test 1 batch

            BRGTools.SetupBatchDrawCommands(drawCmdPt, 1, batchList[0].numInstances);
            batchList[0].DrawBatch(drawCmdPt, batchList[0].visibleIdList.Count);
        }
    }
}
#endif