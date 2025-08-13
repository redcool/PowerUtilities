using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace PowerUtilities
{
    public static class ComputeShaderEx
    {
        static Dictionary<string, ComputeShader> csDict = new();
        /// <summary>
        /// Send var : _DispatchGroupSize
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="kernelId"></param>
        /// <param name="resultTexWidth"></param>
        /// <param name="resultTexHeight"></param>
        /// <param name="resultTexDepth"></param>
        public static void DispatchKernel(this ComputeShader cs,int kernelId,int resultTexWidth,int resultTexHeight,int resultTexDepth)
        {
            cs.GetKernelThreadGroupSizes(kernelId, out var xSize, out var ySize, out var zSize);

            var xGroups = Mathf.CeilToInt(resultTexWidth / (float)xSize);
            var yGroups = Mathf.CeilToInt(resultTexHeight / (float)ySize);
            var zGroups = Mathf.CeilToInt(resultTexDepth / (float)zSize);

            cs.SetFloats("_DispatchGroupSize", xGroups, yGroups, zGroups,0);
            cs.SetFloats("_NumThreads", xSize, ySize, zSize, xSize * ySize * zSize);

            cs.SetCommonTimes();
            cs.Dispatch(kernelId, xGroups, yGroups, zGroups);
        }

        public static void SetCommonTimes(this ComputeShader cs)
        {
            cs.SetFloat("_DeltaTime",Time.deltaTime);
            cs.SetFloat("_Time", Time.time);
        }

        /// <summary>
        /// Dispatch TextureTools.CSClear
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="cs"></param>
        public static void DispatchKernel_CSClear(Texture resultRT, Color clearColor = default)
        {
            var texCS = GetCS("TextureTools");
            var kernel = texCS.FindKernel("CSClear");
            texCS.SetVector("_ClearColor", clearColor);
            texCS.SetTexture(kernel, "_ResultTex", resultRT);
            texCS.DispatchKernel(kernel, resultRT.width, resultRT.height, 1);
        }
        /// <summary>
        /// Dispatch TextureTools.CopyTexture
        /// </summary>
        /// <param name="sourceTex"></param>
        /// <param name="resultRT"></param>
        public static void DispatchKernel_CopyTexture(Texture sourceTex, Texture resultRT)
        {
            var texCS = GetCS("TextureTools");

            var kernel = texCS.FindKernel("CopyTexture");
            texCS.SetTexture(kernel, "_SourceTex", sourceTex);
            texCS.SetVector("_SourceTex_TexelSize", new Vector4(sourceTex.width, sourceTex.height, 1f / sourceTex.width, 1f / sourceTex.height));
            texCS.SetTexture(kernel, "_ResultTex", resultRT);
            texCS.SetVector("_ResultTex_TexelSize", new Vector4(resultRT.width, resultRT.height, 1f / resultRT.width, 1f / resultRT.height));
            texCS.DispatchKernel(kernel, resultRT.width, resultRT.height, 1);
        }

        public static bool CanExecute(this ComputeShader cs)
        => SystemInfo.supportsComputeShaders && cs;

        /// <summary>
        /// Get cs when not ecists
        /// Load computeShader from Resources/Shaders folder 
        /// </summary>
        /// <param name="csName">TextureTools,ColorConvert</param>
        /// <param name="folderName">Resources folder,like: Resources/Shaders/TextureTools.compute</param>
        /// <returns></returns>

        public static ComputeShader GetCS(string csName,string folderName="Shaders")
        {
            return DictionaryTools.Get(csDict, csName, csName => (ComputeShader)Resources.Load($"{folderName}/{csName}") );
        }

    }
}
