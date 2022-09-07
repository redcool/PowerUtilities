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
        public static void SetKeyword(MaterialProperty prop, string localKeyword, bool isKeywordOn)
        {
            var mats = prop.targets.Select(t => (Material)t);
            foreach (var mat in mats)
            {
                if (isKeywordOn)
                    mat.EnableKeyword(localKeyword);
                else
                    mat.DisableKeyword(localKeyword);
            }
        }

        public static void SetKeyword(string globalKeyword,bool isOn)
        {
            if (Shader.IsKeywordEnabled(globalKeyword) == isOn)
                return;

            if (isOn)
                Shader.EnableKeyword(globalKeyword);
            else
                Shader.DisableKeyword(globalKeyword);
        }
    }
}
#endif