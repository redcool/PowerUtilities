#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PowerUtilities
{

    public static class MaterialPropertyTools
    {
        /// <summary>
        /// set material local keyword
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="localKeyword"></param>
        /// <param name="isKeywordOn"></param>
        public static void SetKeyword(this MaterialProperty prop, string localKeyword, bool isKeywordOn)
        {
            var mats = prop.targets.Select(t => (Material)t);
            mats.ForEach(mat => mat.SetKeyword(localKeyword, isKeywordOn));
        }

        public static void SetKeywords(this MaterialProperty prop, bool isKeywordOn, string[] keywords)
        {
            if (keywords == null || keywords.Length == 0)
                return;

            var mats = prop.targets.Select(t => (Material)t);
            mats.ForEach(mat => mat.SetKeywords(keywords, isKeywordOn));
        }

        /// <summary>
        /// Sync Keyword to float materialProperty
        /// 
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="editor"></param>
        /// <param name="keyword"></param>
        /// <param name="isInvertKeyword"></param>
        public static void SyncKeywordToFloat(this MaterialProperty prop, MaterialEditor editor, string keyword,bool isInvertKeyword=false)
        {
            if(string.IsNullOrEmpty(keyword)) return;

            var mat = (Material)editor.target;
            var value = mat.IsKeywordEnabled(keyword) ? 1 : 0;
            
            if(isInvertKeyword)
            {
                value = Mathf.Abs(value-1);
            }

            if (value != prop.floatValue)
            {
                prop.floatValue = value;
            }
        }
    }
}
#endif