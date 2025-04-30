using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.RenderFeatures
{
    /// <summary>
    /// Material shader Lod Info
    /// </summary>
    [Serializable]
    public class ShaderLodInfo
    {
        [Tooltip("update shader max lod")]
        public bool isUpdateShaderMaxLod;
        public List<Material> materialList = new();
        [Tooltip("shader max lod")]
        public int shaderMaxLod = 600;

        public void UpdateShaderLods() {
            foreach (var mat in materialList)
            {
                if (isUpdateShaderMaxLod && mat)
                {
                    mat.shader.maximumLOD = shaderMaxLod;
                }
            }
        }

    }
}
