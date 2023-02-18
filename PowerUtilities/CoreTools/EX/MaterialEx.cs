using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
    }
}
