
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

        [EditorHeader("","Shader Info","0xffffff", indentLevel = 0)]
        //public string shaderInfoHelp="";
        [EditorSettingSO(listPropName = nameof(BRGMaterialInfoListSO.brgMaterialInfoList))]
        public BRGMaterialInfoListSO brgMaterialInfoListSO;

        [Header("CommonCullingGroup")]
        public CommonCullingGroupControl cullingGroupControl;

        /// <summary>
        /// Setup brgGroupInfoList
        /// </summary>
        /// <param name="groupInfos"></param>
        public void SetupBRGGroupInfoList(IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> groupInfos)
        {
            brgGroupInfoList.Clear();

            foreach (IGrouping<(int lightmapId, BatchMeshID meshId, BatchMaterialID matId), MeshRenderer> groupInfo in groupInfos)
            {
                var mat = brg.GetRegisteredMaterial(groupInfo.Key.matId);

                // find material props
                var floatsCount = 0;
                var matPropInfoList = new List<(string name, int floatCount)>();
                // only 2 matrices
                mat.shader.FindShaderPropNames_BRG(ref matPropInfoList, ref floatsCount, false);

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
                    brgGroupInfo.rendererList = subGroupInfo.Select(r => (Renderer)r).ToList();

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

            info.matGroupList.AddRange( cbufferVar.bufferPropList);
            info.brgMaterialInfo = cbufferVar;
        } 
    }
}
