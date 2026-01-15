using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Networking.UnityWebRequest;

namespace PowerUtilities
{
    public static class ComputeShaderEx
    {
        /// <summary>
        /// Compute Names
        /// </summary>
        public const string 
                //Copy Texture
            CS_TEXTURE_TOOLS = "TextureTools",
            // color convert
            CS_COLOR_CONVERT = "ColorConvert"
            ;

        public static Dictionary<string, ComputeShader> csDict = new();
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
        /// SetTexture and set texel size vector4(width,height,1/width,1/height) 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="kernelId"></param>
        /// <param name="texName"></param>
        /// <param name="tex"></param>
        public static void SetTextureWithSize(this ComputeShader cs, int kernelId, string texName,Texture tex)
        {
            cs.SetTexture(kernelId, texName, tex);
            cs.SetVector($"{texName}_TexelSize", new Vector4(tex.width, tex.height, 1f / tex.width, 1f / tex.height));
        }

        /// <summary>
        /// Dispatch TextureTools.CSClear
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="cs"></param>
        public static void DispatchKernel_CSClear(Texture resultRT, Color clearColor = default)
        {
            var texCS = GetCS(CS_TEXTURE_TOOLS);
            var kernel = texCS.FindKernel("CSClear");
            texCS.SetVector("_ClearColor", clearColor);
            texCS.SetTexture(kernel, "_ResultTex", resultRT);
            texCS.DispatchKernel(kernel, resultRT.width, resultRT.height, 1);
        }
        /// <summary>
        /// cs TextureTools 's keyword
        /// </summary>
        /// <param name="texDim"></param>
        /// <returns></returns>
        public static string GetKeyWordTextureTools(TextureDimension texDim) => (texDim) switch
        {
            TextureDimension.Tex3D => "_3D",
            TextureDimension.Tex2DArray => "_2DArr",
            _ => ""
        };
        /// <summary>
        /// Copy sourceTex to resultRT, no worry about different texture size
        /// sourceTex: 2d,2dArray,3d
        /// </summary>
        /// <param name="sourceTex"></param>
        /// <param name="resultRT"></param>
        public static void DispatchKernel_CopyTexture(Texture sourceTex, Texture resultRT, int sourceTexId = 0, int sourceTexLod = 0,float gammaValue=1,int resultTexId=0)
        {
            var texCS = GetCS(CS_TEXTURE_TOOLS);
            var kernel = texCS.FindKernel("CopyTexture");

            // clear keywords
            texCS.enabledKeywords.ForEach(key => texCS.DisableKeyword(key));
            var keyword = GetKeyWordTextureTools(sourceTex.dimension);
            texCS.EnableKeyword(keyword);

            texCS.SetTextureWithSize(kernel, "_SourceTex", sourceTex);
            //texCS.SetTexture(kernel, "_SourceTex", sourceTex);
            //texCS.SetVector("_SourceTex_TexelSize", new Vector4(sourceTex.width, sourceTex.height, 1f / sourceTex.width, 1f / sourceTex.height));

            texCS.SetFloat("_SourceTexId", sourceTexId); // source texture array id
            texCS.SetFloat("_SourceTexLod", sourceTexLod);
            texCS.SetFloat("_GammaValue", gammaValue);

            texCS.SetTextureWithSize(kernel, "_ResultTex", resultRT);
            //texCS.SetTexture(kernel, "_ResultTex", resultRT);
            //texCS.SetVector("_ResultTex_TexelSize", new Vector4(resultRT.width, resultRT.height, 1f / resultRT.width, 1f / resultRT.height));
            texCS.SetFloat("_ResultTexId", resultTexId); // result tex array id

            texCS.DispatchKernel(kernel, resultRT.width, resultRT.height, 1);
        }
        /// <summary>
        /// Copy sourceTex to resultRT, no worry about different texture size
        /// sourceTex: 2d,2dArray,3d
        /// </summary>
        /// <param name="resultTex">The Texture2D that receives the copied texture data. like new Texture2D()</param>
        /// <param name="sourceTex">The source Texture from which to copy data.</param>
        /// <param name="sourceTexId">The texture array slice or ID in the source texture to copy from. The default is 0.</param>
        /// <param name="sourceTexLod">The mipmap level of the source texture to copy. The default is 0.</param>
        /// <param name="gammaValue">The gamma correction value to apply during the copy operation. A value of 1 means no correction. The default
        /// is 1.</param>
        public static void CopyFrom(this Texture2D resultTex, Texture sourceTex, int sourceTexId = 0, int sourceTexLod = 0, float gammaValue = 1)
        {
            var resultRT = RenderTextureTools.GetTemporaryUAV(resultTex.width, resultTex.height, RenderTextureFormat.Default);

            DispatchKernel_CopyTexture(sourceTex, resultRT, sourceTexId, sourceTexLod,gammaValue);
            resultRT.ReadRenderTexture(ref resultTex);
            resultRT.ReleaseSafe();
        }

        public static bool CanExecute(this ComputeShader cs)
        => SystemInfo.supportsComputeShaders && cs;

        /// <summary>
        /// Load computeShader from Resources/Shaders folder 
        /// </summary>
        /// <param name="csName">TextureTools,ColorConvert</param>
        /// <param name="resourceSubFolderName">Resources subfolder,like: Resources/Shaders/TextureTools.compute</param>
        /// <returns></returns>

        public static ComputeShader GetCS(string csName,string resourceSubFolderName="Shaders")
        {
            return DictionaryTools.Get(csDict, csName, csName => (ComputeShader)Resources.Load($"{resourceSubFolderName}/{csName}") );
        }

    }
}
