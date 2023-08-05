using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

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
    }
}
