
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

        [Serializable]
        public class BrgGroupInfo
        {
            [Header("brg")]
            //public BatchMeshID meshId;
            //public BatchMaterialID matId;
            public int lightmapId;
            public int floatsCount;

            [Header("Mesh")]
            public Mesh mesh;

            [Header("Material Info")]
            public Material mat;
            [ListItemDraw("name:,propName,type:,propType,floats:,floatsCount","40,200,40,80,40,")]
            public List<CBufferPropInfo> matGroupList = new();

            [Header("Instance Info")]
            public int instanceCount;
            public List<Renderer> rendererList = new();
        }
        [Header("Draw Info")]
        public List<BrgGroupInfo> brgGroupInfoList = new();

        public List<ShaderCBufferVarInfo> shaderCBufferVarInfoList = new();


        public void SetupBRGGroupInfoList(IEnumerable<IGrouping<(int lightmapIndex, BatchMeshID, BatchMaterialID), MeshRenderer>> groupInfos)
        {
            brgGroupInfoList.Clear();


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

                // analysis shader 's cuffer
                AddShaderCBuffer(brgGroupInfo, shaderCBufferVarInfoList);

                //final calc total buffer floats
                brgGroupInfo.floatsCount = brgGroupInfo.matGroupList.Sum(item => item.floatsCount);

                brgGroupInfoList.Add(brgGroupInfo);
            }
        }

        public static void AddShaderCBuffer(BrgGroupInfo info, List<ShaderCBufferVarInfo> cbufferInfoList)
        {
            var cbufferInfo = cbufferInfoList.Find(cbufferInfo => cbufferInfo.shader == info.mat.shader);
            if (cbufferInfo == null)
                return;

            info.matGroupList.AddRange( cbufferInfo.bufferPropList);
        }
    }
}
