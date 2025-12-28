
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Save shader cbuffer properties
    /// </summary>
    [Serializable]
    public class BRGMaterialInfo : ScriptableObject
    {
        [Tooltip("cbuffer target shader")]
        public Shader shader;
        [Multiline(10)]
        public string shaderCBufferLayoutText;

        [EditorButton(onClickCall ="StartAnalysis",tooltip = "Analysis shaderCBufferLayoutText,fill bufferPropList")]
        public bool isAnalysis;

        [Header("CBuffer info")]
        public List<CBufferPropInfo> bufferPropList = new();

        [EditorButton(onClickCall = "LogDotsMacros",tooltip = "output dots instancing macros for shader")]
        public bool isLogDotsMacros;

        public void StartAnalysis()
        {
            if (!shader || string.IsNullOrWhiteSpace(shaderCBufferLayoutText))
                return;

            bufferPropList = shaderCBufferLayoutText.GetShaderCBufferInfo();

        }

        public void LogDotsMacros()
        {
            var results = GetDotsMacros();
            Debug.Log(results);
        }

        public string GetDotsMacros()
        {
            // fill dots tool macros
            var dotsMacrosSB = new StringBuilder();

            // fill dotsCbuffer
            var sb = new StringBuilder();
            sb.AppendLine(@"#if defined(UNITY_DOTS_INSTANCING_ENABLED)");
            sb.AppendLine("DOTS_CBUFFER_START(MaterialPropertyMetadata)");
            foreach (var propInfo in bufferPropList)
            {
                sb.AppendLine($"\tDEF_VAR({propInfo.propType}, {propInfo.propName})");

                dotsMacrosSB.AppendLine($"\t#define {propInfo.propName} GET_VAR({propInfo.propType}, {propInfo.propName})");
            }
            sb.AppendLine("DOTS_CBUFFER_END");
            sb.AppendLine("");

            sb.Append(dotsMacrosSB.ToString());
            sb.AppendLine("#endif");
            return sb.ToString();
        }

        /// <summary>
        /// Fill datas into graphBuffer
        /// {
        ///     objectToWorld,
        ///     worldToObject,
        ///     mainTex_ST,
        ///     color
        /// }
        /// </summary>
        /// <param name="instId"></param>
        /// <param name="mr"></param>
        public void FillMaterialDatas(BRGBatch brgBatch, int instId, Renderer mr)
        {
            var mat = mr.sharedMaterial;
            var objectToWorld = mr.transform.localToWorldMatrix.ToFloat3x4();
            var worldToObject = mr.transform.worldToLocalMatrix.ToFloat3x4();

            Vector4 mainTex_ST = new float4(mat.mainTextureScale, mat.mainTextureOffset);
            var color = mat.color;

            brgBatch.FillData(objectToWorld.ToColumnArray(), instId, BRGTools.unity_ObjectToWorld);
            brgBatch.FillData(worldToObject.ToColumnArray(), instId, BRGTools.unity_WorldToObject);
            var matPropId = 2;
            foreach (var propInfo in bufferPropList)
            {
                float[] floatArr = mat.GetFloats(propInfo.propName);
                brgBatch.FillData(floatArr, instId, propInfo.propName);
                //Debug.Log($"{propInfo.propName} -> {floatArr.ToString(",")}");
                matPropId++;
            }

        }

        public void FillMaterialData(BRGBatch brgBatch,int instId,string propName, float[] floats)
        {
            var propId = bufferPropList.FindIndex(info => info.propName == propName);
            if(propId < 0)
            {
                throw new Exception($"{propName} not found!");
            }
            brgBatch.FillData(floats,instId, propName);
        }

    }

}
