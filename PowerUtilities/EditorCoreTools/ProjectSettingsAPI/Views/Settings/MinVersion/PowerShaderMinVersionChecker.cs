namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEditor;
    using UnityEngine;

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Tools/PowerShaderMinVersionChecker")]
    [SOAssetPath(nameof(PowerShaderMinVersionChecker))]
    public class PowerShaderMinVersionChecker : ScriptableObject
    {
        [EditorSettingSO(typeof(PowerShaderMinVersionInfoSO))]
        public PowerShaderMinVersionInfoSO minVersionInfo;


        static bool IsKeywordsValid(string[] keywords, string[] materialKeywords)
        {
            if(materialKeywords == null || materialKeywords.Length == 0) { return false; }

            return IsValid(materialKeywords, materialKeyword => keywords.Contains(materialKeyword));
        }

        private static bool IsPropValid(string[] props, Material m)
        {
            if (props == null || props.Length == 0) return false;

            return IsValid(props, propName => m.GetFloat(propName) != 0);
        }


        static bool IsValid(IEnumerable<string> keys, Func<string, bool> onCheck)
        {
            foreach (var name in keys)
            {
                if (onCheck(name)) return false;
            }
            return true;
        }

        static bool IsValidMinVersion(string[] keywords, string[] props, Material m,
            bool isKeywordValidReverse = false,
            bool isPropValidReverse = false)
        {
            var isPropValid = IsPropValid(props, m);
            if (isPropValidReverse)
                isPropValid = !isPropValid;

            var isKeywordValid = IsKeywordsValid(keywords, m.shaderKeywords);
            if (isKeywordValidReverse)
                isKeywordValid = !isKeywordValid;

            return isKeywordValid || isPropValid;
        }

        public static int CheckMinVersion(string folder, Shader shader, string[] keywords, string[] props,
            string minVersionKeyword = "MIN_VERSION",
            bool isKeywordValidReverse = false,
            bool isPropValidReverse = false)
        {
            if (!shader || (keywords == null && props == null))
                return 0;

            var list = AssetDatabaseTools.FindAssetsInProject<Material>("t:material", folder);
            var q = list.Where(m => m.shader == shader);
            var count = 0;
            q.ForEach(m =>
            {
                if (IsValidMinVersion(keywords, props, m, isKeywordValidReverse, isPropValidReverse))
                {
                    //m.SetFloat("_MinVersion",1);
                    m.EnableKeyword(minVersionKeyword);
                    count++;
                }

            });
            return count;
        }
    }
}
