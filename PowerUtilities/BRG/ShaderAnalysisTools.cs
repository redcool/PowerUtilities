using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace PowerUtilities
{
    public static class ShaderAnalysisTools

    {
        public static void Analysis(this Shader shader)
        {
#if UNITY_EDITOR
            //var sd = ShaderUtil.GetShaderData(shader);
            //for (var i = 0; i < sd.SubshaderCount; i++)
            //{
            //    var subShader = sd.GetSubshader(i);
            //    for (int j = 0; j < subShader.PassCount; j++)
            //    {
            //        var pass = subShader.GetPass(j);
            //        var compileInfo = pass.CompileVariant(UnityEditor.Rendering.ShaderType.Vertex, new[] { "DOTS_INSTANCING_ON" }, UnityEditor.Rendering.ShaderCompilerPlatform.GLES3x, BuildTarget.Android);
            //        foreach (var cb in compileInfo.ConstantBuffers)
            //        {
            //            Debug.Log($"---- {cb.Name} ,size:{cb.Size}");

            //            foreach (var f in cb.Fields)
            //            {
            //                Debug.Log($"{f.Name},{f.DataType}");
            //            }
            //        }
            //    }

            //}
#endif
        }
    }
}
