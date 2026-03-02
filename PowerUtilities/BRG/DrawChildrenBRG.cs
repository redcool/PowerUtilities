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

        [Tooltip("max instance count in a group")]
        public int maxCountPerGroup = 9999; // gles3 will auto set

        [Tooltip("normalMap need float3x4 : unity_WorldToObject ")]
        public bool isRenderNormalMap = true;
        public bool isFindShaderProp = true;
        public bool isSkipTexST;

        [EditorButton(onClickCall = nameof(RecordChildren))]
        public bool isRecord;

        [EditorButton(onClickCall = nameof(Clear))]
        public bool isClear;

        [Header("Draw Info")]
        public List<BrgGroupInfo> brgGroupInfoList = new();

        [EditorHeader("", "Shader Info", "0xffffff", indentLevel = 0)]
        //public string shaderInfoHelp="";
        [EditorSettingSO(listPropName = nameof(BRGMaterialInfoListSO.brgMaterialInfoList))]
        public BRGMaterialInfoListSO brgMaterialInfoListSO;

        [Header("CommonCullingGroup")]
        public CommonCullingGroupControl cullingGroupControl;

        BatchRendererGroup brg;

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
                RecordChildren();
            }
            FillBatchList(brgGroupInfoList);

            SetupCommonCullingGroupEvents();
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
        public void RecordChildren()
        {
            if(!rootGo)
                rootGo = gameObject;

            CheckBRGMaterialInfoListSO();
            if (brg == null)
                brg = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);

            var groupInfos = GetChildrenGroups(rootGo);
            SetupBRGGroupInfoList(groupInfos);
            FillBatchList(brgGroupInfoList);

            SetupCommonCullingGroupEvents();
        }

        public void Clear()
        {
            foreach (var brgBatch in batchList)
            {
                brgBatch.Dispose();
            }
            batchList.Clear();
            brgGroupInfoList.Clear();
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
        public IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), Renderer>> GetChildrenGroups(GameObject rootGO)
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
        /// Fill batchList from brgGroupInfo(RecordChildren first)
        /// </summary>
        public void FillBatchList(List<BrgGroupInfo> brgGroupInfoList)
        {
            batchList.Clear();
            batchList.AddRange(
                brgGroupInfoList.Select((brgGroupInfo, groupId) =>
                {
                    var meshId = brg.RegisterMesh(brgGroupInfo.mesh);
                    var matId = brg.RegisterMaterial(brgGroupInfo.mat);

                    var brgBatch = new BRGBatch(brg, brgGroupInfo.instanceCount, meshId, matId, groupId);
                    brgBatch.brgMaterialInfo = brgGroupInfo.brgMaterialInfo;

                    brgBatch.Setup(
                        brgGroupInfo.matGroupList.Select(matInfo => (matInfo.propName,matInfo.floatsCount)).ToArray()
                        );

                    brgBatch.FillMaterialDataAndSetupBatchBlock(brgGroupInfo.rendererList, brgGroupInfo.brgMaterialInfo.FillMaterialDatas);
                    //brgBatch.visibleIdList = brgGroupInfo.visibleIdList; // same ref

                    return brgBatch;
                })
            );

        }

        public void SetupCommonCullingGroupEvents()
        {
            if (!cullingGroupControl)
                return;

            cullingGroupControl.OnStateChanged -= CullingGroupControl_OnStateChanged;
            cullingGroupControl.OnStateChanged += CullingGroupControl_OnStateChanged;
        }

        private void CullingGroupControl_OnStateChanged(CommomCullingInfo info)
        {
            if (batchList.Count <= info.batchGroupId)
                return;

            var brgBatch = batchList[info.batchGroupId];
            // remove target id first
            brgBatch.visibleIdList.Remove(info.visibleId);

            if (info.IsVisible)
                brgBatch.visibleIdList.Add(info.visibleId);

            //Debug.Log($"visible changed: groupid: {info.batchGroupId}, visibleId:{info.visibleId}");
        }

        public unsafe JobHandle OnPerformCulling(
            BatchRendererGroup rendererGroup,
            BatchCullingContext cullingContext,
            BatchCullingOutput cullingOutput,
            IntPtr userContext)
        {
            var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

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

        /// <summary>
        /// Setup brgGroupInfoList
        /// </summary>
        /// <param name="groupInfos"></param>
        public void SetupBRGGroupInfoList(IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), Renderer>> groupInfos)
        {
            brgGroupInfoList.Clear();

            foreach (IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), Renderer> groupInfo in groupInfos)
            {
                var mat = brg.GetRegisteredMaterial(groupInfo.Key.matId);

                // find material props
                var floatsCount = 0;
                var matPropInfoList = new List<(string name, int floatCount)>();
                // only 2 matrices
                mat.shader.FindShaderPropNames_BRG(ref matPropInfoList, ref floatsCount, false, hasNormalMap: isRenderNormalMap);

                var mesh = brg.GetRegisteredMesh(groupInfo.Key.meshId);

                var subGroupInfos = groupInfo.Chunk(maxCountPerGroup);
                foreach (var subGroupInfo in subGroupInfos)
                {
                    var instCount = subGroupInfo.Count();

                    var brgGroupInfo = new BrgGroupInfo
                    {
                        mesh = mesh,
                        mat = mat,
                        instanceCount = instCount,
                        lightmapId = groupInfo.Key.lightmapId,
                    };

                    //----- get mat prop infos
                    brgGroupInfo.matGroupList.AddRange(
                        matPropInfoList.Select(propInfo =>
                            new CBufferPropInfo()
                            {
                                floatsCount = propInfo.floatCount,
                                propName = propInfo.name
                            }
                        )
                    );
                    // iterate renderers
                    brgGroupInfo.rendererList = subGroupInfo.ToList();

                    // analysis shader others material props
                    AddShaderCBuffer(brgGroupInfo, brgMaterialInfoListSO?.brgMaterialInfoList);

                    //final calc total buffer floats
                    brgGroupInfo.floatsCount = brgGroupInfo.matGroupList.Sum(item => item.floatsCount);
                    brgGroupInfo.groupName = $"{mesh.name}_{mat.name}_{instCount}";
                    brgGroupInfoList.Add(brgGroupInfo);
                }
            }
        }

        public static void AddShaderCBuffer(BrgGroupInfo info, List<BRGMaterialInfo> matInfoList)
        {
            if (matInfoList == null)
                return;

            var cbufferVar = matInfoList.Find(matInfo => matInfo.shader == info.mat.shader);
            if (cbufferVar == null)
            {
                throw new Exception($"{info.mat.shader} cbuffer info not found,check {nameof(DrawChildrenBRG)}.;shaderCBufferVarListSO");
            }

            info.matGroupList.AddRange(cbufferVar.bufferPropList);
            info.brgMaterialInfo = cbufferVar;
        }
    }
}
#endif