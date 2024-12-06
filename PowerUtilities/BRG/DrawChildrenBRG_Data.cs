
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
        public class MatPropInfo
        {
            public string propName;
            public int propFloatCount;
        }
        [Serializable]
        public class BrgGroupInfo
        {
            [Header("brg")]
            public BatchMeshID meshId;
            public BatchMaterialID matId;
            public int floatsCount;

            [Header("Mesh")]
            public Mesh mesh;

            [Header("Material Info")]
            public Material mat;
            public List<MatPropInfo> matGroupList = new();

            [Header("Instance Info")]
            public int instanceCount;
            public List<Renderer> rendererList = new();
        }
        [Serializable]
        public class ShaderCBufferVarInfo
        {
            public Shader shader;
            [Multiline]
            public string shaderCBufferLayoutText;
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

                mat.shader.FindShaderPropNames_BRG(ref matPropNameList, ref floatsCount, propFloatCountList);
                ShaderAnalysisTools.Analysis(mat.shader);

                foreach (var matPropName in matPropNameList)
                    Debug.Log(matPropName);

                var mesh = brg.GetRegisteredMesh(groupInfo.Key.meshId);

                var brgGroupInfo = new BrgGroupInfo
                {
                    mesh = mesh,
                    mat = mat,
                    instanceCount = instCount,
                    matId = groupInfo.Key.matId,
                    meshId = groupInfo.Key.meshId,
                    floatsCount = propFloatCountList.Sum()
                };
                brgGroupInfoList.Add(brgGroupInfo);

                //----- get mat prop infos
                brgGroupInfo.matGroupList.AddRange(
                    matPropNameList.Select((propName, id) =>
                    {
                        var propFloutCount = propFloatCountList[id];
                        return new MatPropInfo()
                        {
                            propFloatCount = propFloutCount,
                            propName = propName,
                        };
                    })
                );

                // iterate renderers
                brgGroupInfo.rendererList = groupInfo.Select(r => (Renderer)r).ToList();

                // analysis shader 's cuffer
                AnalysisShaderCBuffer(brgGroupInfo,shaderCBufferVarInfoList);
            }
        }

        public static void AnalysisShaderCBuffer(BrgGroupInfo info, List<ShaderCBufferVarInfo> cbufferInfoList)
        {
            var cbufferInfo = cbufferInfoList.Find(cbufferInfo => cbufferInfo.shader == info.mat.shader);
            if (cbufferInfo == null)
                return;

            info.matGroupList.Clear();

            foreach (var line in cbufferInfo.shaderCBufferLayoutText.ReadLines())
            {
                foreach (Match m in Regex.Matches(line, RegExTools.CBUFFER_VARS))
                {
                    if (m.Groups.Count == 3)
                        Debug.Log(m.Groups[1] + ":" + m.Groups[2]);
                }
            }
        }
    }
}
