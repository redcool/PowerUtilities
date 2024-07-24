using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class MaterialEx
    {
        public static void SetKeyword(this Material mat, string keyword, bool isOn)
        {
            if (string.IsNullOrEmpty(keyword))
                return;

            var isOpened = mat.IsKeywordEnabled(keyword);
            if (isOn == isOpened)
                return;

            if (isOn)
                mat.EnableKeyword(keyword);
            else
                mat.DisableKeyword(keyword);
        }

        public static void SetKeywords(this Material mat,string[] keywords, bool isOn)
        {
            if(keywords== null || keywords.Length == 0) return;
            foreach (var item in keywords)
            {
                SetKeyword(mat,item,isOn);
            }
        }

        public static void SetKeywords(this Material mat, bool isOn,params string[] keywords)
        {
            SetKeywords(mat, keywords, isOn);
        }

        #region Sync Set Material and MaterialPropertyBlock


        public static void SetInt(this Material mat, string name, int value, MaterialPropertyBlock block)
        => SetInt(mat, Shader.PropertyToID(name), value, block);

        public static void SetInt(this Material mat, int nameId, int value, MaterialPropertyBlock block)
        {
            mat.SetInt(nameId, value);
            if (block != null) block.SetInt(nameId, value);
        }

        /// <summary>
        /// Set material and block
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="block"></param>
        public static void SetFloat(this Material mat, string name, float value, MaterialPropertyBlock block)
            => SetFloat(mat, Shader.PropertyToID(name), value, block);

        public static void SetFloat(this Material mat, int nameId, float value, MaterialPropertyBlock block)
        {
            mat.SetFloat(nameId, value);
            if (block != null)
                block.SetFloat(nameId, value);
        }

        public static void SetFloatArray(this Material mat, string name, float[] values, MaterialPropertyBlock block)
            => SetFloatArray(mat, Shader.PropertyToID(name), values, block);

        public static void SetFloatArray(this Material mat, int nameId, float[] values, MaterialPropertyBlock block)
        {
            mat.SetFloatArray(nameId, values);
            if (block != null)
                block.SetFloatArray(nameId, values);
        }

        public static void SetTexture(this Material mat, string name, Texture value, MaterialPropertyBlock block)
            => SetTexture(mat, Shader.PropertyToID(name), value, block);

        public static void SetTexture(this Material mat, int nameId, Texture value, MaterialPropertyBlock block)
        {
            mat.SetTexture(nameId, value);
            if (block != null)
                block.SetTexture(nameId, value);
        }

        public static void SetVector(this Material mat, string name, Vector4 value, MaterialPropertyBlock block)
            => SetVector(mat, Shader.PropertyToID(name), value, block); 

        public static void SetVector(this Material mat, int nameId, Vector4 value, MaterialPropertyBlock block)
        {
            mat.SetVector(nameId, value);
            if (block != null) block.SetVector(nameId, value);
        }

        public static void SetVectorArray(this Material mat, string name, Vector4[] value, MaterialPropertyBlock block)
            => SetVectorArray(mat, Shader.PropertyToID(name), value, block);

        public static void SetVectorArray(this Material mat, int nameId, Vector4[] value, MaterialPropertyBlock block)
        {
            mat.SetVectorArray(nameId, value);
            if (block != null) block.SetVectorArray(nameId, value);
        }

        public static void SetMatrix(this Material mat, string name, Matrix4x4 value, MaterialPropertyBlock block)
            => SetMatrix(mat, Shader.PropertyToID(name), value, block);

        public static void SetMatrix(this Material mat, int nameId, Matrix4x4 value, MaterialPropertyBlock block)
        {
            mat.SetMatrix(nameId, value);
            if (block != null) block.SetMatrix(nameId, value);
        }

        public static void SetMatrixArray(this Material mat, string name, Matrix4x4[] value, MaterialPropertyBlock block)
            => SetMatrixArray(mat, Shader.PropertyToID(name), value, block);

        public static void SetMatrixArray(this Material mat, int nameId, Matrix4x4[] value, MaterialPropertyBlock block)
        {
            mat.SetMatrixArray(nameId, value);
            if (block != null) block.SetMatrixArray(nameId, value);
        }

        public static void SetColor(this Material mat, string name, Color value, MaterialPropertyBlock block)
            => SetColor(mat, Shader.PropertyToID(name), value, block);

        public static void SetColor(this Material mat, int nameId, Color value, MaterialPropertyBlock block)
        {
            mat.SetColor(nameId, value);
            if (block != null)
                block.SetColor(nameId, value);
        }

        #endregion

        public static void SetStencil(this Material mat, CompareFunction comp, int stencilValue,
            string stencilPropName = "_Stencil", string stencilCompPropName = "_StencilComp")
        {
            if (!mat)
                return;

            if (mat.HasProperty(stencilPropName))
                mat.SetFloat(stencilPropName, stencilValue);
            if (mat.HasProperty(stencilCompPropName))
                mat.SetFloat(stencilCompPropName, (int)comp);
        }
    }
}
