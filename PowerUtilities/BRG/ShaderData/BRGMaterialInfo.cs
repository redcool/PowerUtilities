
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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


        [Tooltip("Remove Props not included in shader property block")]
        [Multiline(10)]
        public string shaderCBufferLayoutText;

        [Tooltip("Skip like _MainTex_ST,brg rendering use default(1,1,0,0)")]
        public bool isSkipTexST = true; 

        [EditorBox("Buttons", "isAnalysis,isAnalysisShader",boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall =nameof(StartAnalysisCBufferLayoutText),tooltip = "Analysis shaderCBufferLayoutText,fill bufferPropList")]
        public bool isAnalysis;

        [EditorButton(onClickCall = nameof(StartAnalysisShader), tooltip = "Analysis shader,fill bufferPropList")]
        [HideInInspector]
        public bool isAnalysisShader;

        [Header("CBuffer info")]
        public List<CBufferPropInfo> bufferPropList = new();

        [EditorButton(onClickCall = "LogDotsMacros",tooltip = "output dots instancing macros for shader")]
        public bool isOutputDotsMacrosCode;

        public void StartAnalysisCBufferLayoutText()
        {
            if (!shader || string.IsNullOrWhiteSpace(shaderCBufferLayoutText))
                return;

            bufferPropList = shaderCBufferLayoutText.GetShaderCBufferInfo(isSkipTexST);
        }
        public void StartAnalysisShader()
        {
            var list = new List<(string name, int count)>();
            var floatsCount = 0;
            shader.FindShaderPropNames(ref list, ref floatsCount, isSkipTexST);
            bufferPropList = list.Select(x => new CBufferPropInfo
            {
                propName = x.name,
                floatsCount = floatsCount,
                propType = "float"+x.count
            }).ToList();
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

            var startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_ObjectToWorld, 12, instId, 1);
            brgBatch.instanceBuffer.SetData(objectToWorld, 0, startId);

            startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_WorldToObject, 12, instId, 1);
            brgBatch.instanceBuffer.SetData(worldToObject, 0, startId);

            // per float fill instanceBuffer
            foreach (var propInfo in bufferPropList)
            {
                float[] floatArr = mat.GetFloats(propInfo.propName);

                startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, propInfo.propName, 1, instId, floatArr.Length);
                if(startId == -1)
                    throw new Exception($"{propInfo.propName} not found in shader cbuffer layout");

                brgBatch.instanceBuffer.SetData(floatArr, 0, startId,floatArr.Length
                    );
                //Debug.Log($"{propInfo.propName} -> {floatArr.ToString(",")}");
            }

        }

        public void FillMaterialData(BRGBatch brgBatch,int instId,string propName, float[] floats)
        {
            var prop = bufferPropList.Find(info => info.propName == propName);
            if(prop == null)
            {
                throw new Exception($"{propName} not found!");
            }

            brgBatch.instanceBuffer.SetData(floats, 0, 
                BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict,propName,prop.floatsCount,instId,1),
                floats.Length);

        }

    }

}
