
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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

            [Header("Unity Params")]
            public Mesh mesh;
            public Material mat;
            public List<MatPropInfo> matGroupList = new();
            public int instanceCount;
            public List<Renderer> rendererList = new();
        }
        [Header("DebugInfo")]
        public List<BrgGroupInfo> brgGroupInfoList = new();


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
            }
        }

        public void AnalysisShader()
        {

        }
    }
}
