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

        /// <summary>
        /// Check material's keyword and uniform variables states
        /// like powervfx, [GroupToggle(,KEY_1)]
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="toggleTypeString"></param>
        /// <param name="mats">materials use shader</param>
        /// <returns></returns>
        public static string SyncMaterialKeywords(this Shader shader, string toggleTypeString, params Material[] mats)
        {
            var propKeywordList = shader.GetShaderPropsHasKeyword(toggleTypeString);

            var result = new StringBuilder();

            foreach (var mat in mats)
            {
                foreach (var propKeyword in propKeywordList)
                {
                    // material dont has property
                    if (! mat.HasProperty(propKeyword.propName))
                    {
                        result.AppendLine($"{mat.name} , {propKeyword.propName} not existed => {propKeyword.keyword}");
                        mat.DisableKeyword(propKeyword.propName);
                        continue;
                    }
                    var keywordOn = mat.GetFloat(propKeyword.propName) != 0;
                    // keyword has changed
                    if (mat.IsKeywordEnabled(propKeyword.keyword) != keywordOn)
                    {
                        result.AppendLine($"{mat.name} , {propKeyword.propName} => {propKeyword.keyword}");
                    }

                    // update keyword
                    if (keywordOn)
                        mat.EnableKeyword(propKeyword.keyword);
                    else
                        mat.DisableKeyword(propKeyword.keyword);

                }
            }
            return result.ToString();
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
        ///like:
        //{
        //    "unity_ObjectToWorld", //12 floats
        //    "unity_WorldToObject", //12
        //    "_Color", //4
        //};
        //var floatsCount = 12 + 12 + 4;
        /// </summary>
        /// <param name="shader"></param>
        /// <param name="propInfoList">property name list</param>
        /// <param name="floatsCount">all properties float count</param>
        /// <param name="propFloatCountList">property float count</param>
        public static void FindShaderPropNames(this Shader shader, ref List<(string name, int floatCount)> propInfoList, ref int floatsCount)
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

                propInfoList.Add((propName,propFloatCount));
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

        /// <summary>
        /// Get CBufferPropInfo list from cbufferVariableString
        /// </summary>
        /// <param name="cbufferVariableString"> float4 _Color; float4 _Color1, _Color2;</param>
        /// <returns></returns>
        public static List<CBufferPropInfo> GetShaderCBufferInfo(this string cbufferVariableString)
        {
            /*
            float4 _Color;
            float4 _Color1, _Color2;
             */
            const string PATTERN_DEF_VARIABLE = @"(\w+)";

            List<CBufferPropInfo> list = new();
            foreach (var line in cbufferVariableString.ReadLines())
            {
                var ms = Regex.Matches(line, PATTERN_DEF_VARIABLE);
                var varType = ms?.First().Value;

                foreach (Match valueMatch in ms?.Skip(1))
                {
                    {
                        var varName = valueMatch.Value;
                        var floats = CalcFloatsCount(varType);
                        list.Add(new CBufferPropInfo
                        {
                            propType = varType,
                            propName = varName,
                            floatsCount = floats,
                            propNameId = Shader.PropertyToID(varName),
                        });
                    }
                }
            }
            return list;
        }
    }
}
