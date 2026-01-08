#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    [ExecuteInEditMode]
    public partial class DrawChildrenBRG : MonoBehaviour
    {
        public GameObject rootGo;
        [Tooltip("Find meshRenderer from children include invisible")]
        public bool isIncludeInvisible = false;

        public int maxCountPerGroup = 583; // gles3

        [EditorButton(onClickCall = nameof(RecordChildren))]
        public bool isRecord;

        BatchRendererGroup brg;

        //IEnumerable<(GraphicsBuffer, IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer>)> drawInfos;
        // for rendering
        List<BRGBatch> batchList = new();


        void OnEnable()
        {
            if (!rootGo)
                rootGo = gameObject;

            CheckBRGMaterialInfoListSO();
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            if (brgGroupInfoList.Count == 0)
            {
                var groupInfos = GetChildrenGroups(rootGo);
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
            if(!rootGo)
                rootGo = gameObject;

            CheckBRGMaterialInfoListSO();
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            var groupInfos = GetChildrenGroups(rootGo);
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
        public IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> GetChildrenGroups(GameObject rootGO)
        {
            var mrs = rootGo.GetComponentsInChildren<MeshRenderer>(isIncludeInvisible);
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
            batchList.Clear();

            foreach (IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer> groupInfo in groupInfos)
            {
                // find material props
                var matPropInfoList = new List<(string name, int floatCount)>();
                var floatsCount = 0;
                var mat = brg.GetRegisteredMaterial(groupInfo.Key.matId);
                mat.shader.FindShaderPropNames_BRG(ref matPropInfoList, ref floatsCount);
                var matPropInfoArr = matPropInfoList.ToArray();

                // gles3,need 
                maxCountPerGroup = BRGTools.GetMaxInstanceCount(maxCountPerGroup, floatsCount * 4);

                var renderersGroups = groupInfo.Chunk(maxCountPerGroup).ToArray();

                var groupId = 0;
                foreach (var renderers in renderersGroups)
                {
                    var instCount = renderers.Length;

                    var brgBatch = new BRGBatch(brg, instCount, groupInfo.Key.meshId, groupInfo.Key.matId, groupId);
                    //setup shaderCBufferVar
                    var brgMaterialInfo = brgMaterialInfoListSO.brgMaterialInfoList.Find(bufferVar => bufferVar.shader == mat.shader);
                    brgBatch.brgMaterialInfo = brgMaterialInfo;

                    brgBatch.Setup(floatsCount, matPropInfoArr);
                    brgBatch.FillMaterialDataAndSetupBatchBlock(renderers, brgMaterialInfo.FillMaterialDatas);

                    batchList.Add(brgBatch);
                    groupId++;
                }
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
                    brgBatch.visibleIdList = brgGroupInfo.visibleIdList; // same ref

                    return brgBatch;
                })
            );

        }

        public void SetupCommonCullingGroup()
        {
            for (int i = 0; i < brgGroupInfoList.Count; i++)
            {
                var isClear = i == 0;
                var isSetBoundingSpheres = i == brgGroupInfoList.Count - 1;
                var brgGroupInfo = brgGroupInfoList[i];
                //add all  visible ids
                brgGroupInfo.visibleIdList.Clear();
                for (int j = 0; j < brgGroupInfo.rendererList.Count; j++)
                    brgGroupInfo.visibleIdList.Add(j);
                // culling ids
                cullingGroupControl?.SetupCullingInfos(brgGroupInfo.rendererList, isClear, isSetBoundingSpheres,i);
            }
            if (!cullingGroupControl)
                return;

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