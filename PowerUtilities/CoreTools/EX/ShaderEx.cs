using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public static class ShaderEx
    {
        /// <summary>
        /// Get shader props which has keyword GroupToggle
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="toggleTypeName"></param>
        /// <returns></returns>
        public static List<(string propName, string keyword)> GetShaderPropsHasKeyword(this Shader shader, string toggleTypeName = "GroupToggle")
        {
            var list = new List<(string propName, string keyword)>();
            var getKeywordRegex = new Regex(@",(\w+)");

            var t = shader.GetType();
            var propCount = shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                var propAttr = shader.GetPropertyAttributes(i)
                    .Where(pa => pa.StartsWith(toggleTypeName)) //like [GroupToggle(_,KEY1)]
                    .FirstOrDefault();

                if (propAttr == null)
                    continue;

                var match = getKeywordRegex.Match(propAttr);
                if (!match.Success)
                    continue;

                var keywordGroup = match.Groups[1];

                list.Add((shader.GetPropertyName(i), keywordGroup.Value));
            }
            return list;
        }

        public static void SetKeywords(bool isOn,params string[] keywords)
        {
            if (keywords== null || keywords.Length == 0) return;
            foreach (var item in keywords)
            {
                if (Shader.IsKeywordEnabled(item) == isOn)
                    continue;

                if (isOn)
                    Shader.EnableKeyword(item);
                else
                    Shader.DisableKeyword(item);
            }
        }

        /// <summary>
        /// get materialProperty's attributes
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string[] GetPropertyAttributes(this Shader shader, string propName)
        {
            var propIndex = shader.FindPropertyIndex(propName);
            return shader.GetPropertyAttributes(propIndex);
        }

        /// <summary>
        /// materialProperty 's attributes has targetAttribute?
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="propName"></param>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static bool HasPropertyAttribute(this Shader shader, string propName, string attributeName)
        {
            var attrs = GetPropertyAttributes(shader, propName);
            foreach (var attr in attrs)
            {
                if(attr.Contains(attributeName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// PropType ,need how many float numbers
        /// </summary>
        /// <param name="propType"></param>
        /// <returns></returns>
        public static int GetFloatCount(ShaderPropertyType propType) => propType switch
        {
            ShaderPropertyType.Float => 1,
            ShaderPropertyType.Range => 1,
            ShaderPropertyType.Int => 1,
            ShaderPropertyType.Color => 4,
            ShaderPropertyType.Vector => 4,
            _ => 0,
        };

        /// <summary>
        /// Find shader propNames,and need how many floats
        /// 
        /// cause incorrect when _Texture not match _Texture_ST
        /// 
        ///likke:
        //{
        //    "unity_ObjectToWorld", //12 floats
        //    "unity_WorldToObject", //12
        //    "_Color", //4
        //};
        //var floatsCount = 12 + 12 + 4;
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="floatsCount"></param>
        /// <returns></returns>
        public static void FindShaderPropNames(this Shader shader, ref List<string> propNameList, ref int floatsCount, List<int> propFloatCountList)
        {
            var propCount = shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                var propName = shader.GetPropertyName(i);
                //var propNameId = shader.GetPropertyNameId(i);
                var propType = shader.GetPropertyType(i);
                if (propType == ShaderPropertyType.Texture)
                {
                    // texture,find tex_ST
                    propName += "_ST";
                    propType = ShaderPropertyType.Vector;
                }

                var propFloatCount = GetFloatCount(propType);
                floatsCount += propFloatCount;

                propNameList.Add(propName);

                if (propFloatCountList != null)
                    propFloatCountList.Add(propFloatCount);
            }

        }

        public static int CalcFloatsCount(string propType)
        {
            int floats = 1;
            var ms = Regex.Matches(propType, RegExTools.NUMBER);
            foreach (Match m in ms)
            {
                var num = Convert.ToInt32(m.Value);
                floats *= num;
            }
            return floats;
        }

        public static List<CBufferPropInfo> GetShaderCBufferInfo(this string cbufferDef)
        {
            List<CBufferPropInfo> list = new();
            foreach (var line in cbufferDef.ReadLines())
                foreach (Match m in Regex.Matches(line, RegExTools.CBUFFER_VARS))
                {
                    if (m.Groups.Count == 3)
                    {
                        var varType = m.Groups[1].Value;
                        var varName = m.Groups[2].Value;
                        var floats = CalcFloatsCount(varType);
                        list.Add(new CBufferPropInfo
                        {
                            propType = varType,
                            propName = varName,
                            floatsCount = floats
                        });
                    }
                }
            return list;
        }
    }
}
