
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Rendering;

    public partial class DrawChildrenBRG
    {
        [Header("Draw Info")]
        public List<BrgGroupInfo> brgGroupInfoList = new();

        [Header("Shader Info")]
        public string shaderInfoHelp="";
        [EditorSettingSO(listPropName = nameof(BRGMaterialInfoListSO.brgMaterialInfoList))]
        public BRGMaterialInfoListSO shaderCBufferVarListSO;

        [Header("CommonCullingGroup")]
        public CommonCullingGroupControl cullingGroupControl;

        /// <summary>
        /// Setup brgGroupInfoList
        /// </summary>
        /// <param name="groupInfos"></param>
        public void SetupBRGGroupInfoList(IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> groupInfos)
        {
            brgGroupInfoList.Clear();

            var instanceIdStart = 0;
            foreach (IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer> groupInfo in groupInfos)
            {
                var instCount = groupInfo.Count();

                var mat = brg.GetRegisteredMaterial(groupInfo.Key.matId);

                // find material props
                var floatsCount = 0;
                var matPropNameList = new List<string>();
                var propFloatCountList = new List<int>();

                mat.shader.FindShaderPropNames_BRG(ref matPropNameList, ref floatsCount, propFloatCountList,false);

                var mesh = brg.GetRegisteredMesh(groupInfo.Key.meshId);

                var brgGroupInfo = new BrgGroupInfo
                {
                    mesh = mesh,
                    mat = mat,
                    instanceCount = instCount,
                    lightmapId = groupInfo.Key.lightmapId,
                };
                
                // sum instanceIdStart
                instanceIdStart += instCount;

                //----- get mat prop infos
                brgGroupInfo.matGroupList.AddRange(
                    matPropNameList.Select((propName, id) =>
                    {
                        var propFloutCount = propFloatCountList[id];
                        return new CBufferPropInfo()
                        {
                            floatsCount = propFloutCount,
                            propName = propName,
                        };
                    })
                );

                // iterate renderers
                brgGroupInfo.rendererList = groupInfo.Select(r => (Renderer)r).ToList();
                // visibleIdList set empty default
                //brgGroupInfo.visibleIdList = Enumerable.Range(0, brgGroupInfo.rendererList.Count).ToList();
                // analysis shader 's cuffer
                AddShaderCBuffer(brgGroupInfo, shaderCBufferVarListSO?.brgMaterialInfoList);

                //final calc total buffer floats
                brgGroupInfo.floatsCount = brgGroupInfo.matGroupList.Sum(item => item.floatsCount);
                brgGroupInfo.groupName = $"{mesh.name}_{mat.name}_{instCount}";
                brgGroupInfoList.Add(brgGroupInfo);
            }
        }

        public static void AddShaderCBuffer(BrgGroupInfo info, List<BRGMaterialInfo> cbufferVarList)
        {
            if (cbufferVarList == null)
                return;

            var cbufferVar = cbufferVarList.Find(cbufferInfo => cbufferInfo.shader == info.mat.shader);
            if (cbufferVar == null)
            {
                throw new Exception($"{info.mat.shader} cbuffer info not found,check {nameof(DrawChildrenBRG)}.;shaderCBufferVarListSO");
            }

            info.matGroupList.AddRange( cbufferVar.bufferPropList);
            info.shaderCBufferVar = cbufferVar;
        } 
    }
}
