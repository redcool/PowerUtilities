
namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Save shader cbuffer properties
    /// </summary>
    [Serializable]
    public class ShaderCBufferVarInfo : ScriptableObject
    {
        public Shader shader;
        [Multiline()]
        public string shaderCBufferLayoutText;
        [EditorButton(onClickCall ="StartAnalysis")]
        public bool isAnalysis;

        [Header("CBuffer info")]
        public List<CBufferPropInfo> bufferPropList = new();


        void StartAnalysis()
        {
            if (!shader || string.IsNullOrWhiteSpace(shaderCBufferLayoutText))
                return;

            bufferPropList = shaderCBufferLayoutText.GetShaderCBufferInfo();

        }
    }

}
