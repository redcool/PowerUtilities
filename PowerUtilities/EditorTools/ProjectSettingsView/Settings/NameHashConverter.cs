namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
#if UNITY_EDITOR
    using UnityEditor;
#endif
    using UnityEngine;
    using Object = UnityEngine.Object;

    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/Tools/NameHashConverter")]
    [SOAssetPath("Assets/PowerUtilities/NameHashConverter.asset")]
    public class NameHashConverter : ScriptableObject
    {

        public enum HashType
        {
            Animator,
            ShaderProperty,
        }
        [Serializable]
        public class NameHashInfo
        {
            public HashType hashType;
            public string name;
            public int hash;
            public void Clear()
            {
                name = string.Empty;
                hash = 0;
            }
        }
        [HelpBox]
        public string helpBox = "string hash convert";

        [ListItemDraw("type:,hashType,name:,name,hash:,hash","35,.2,35,.3,35,.36")]
        public List<NameHashInfo> list = new();

        [EditorButton(onClickCall = nameof(OnConvert))]
        public bool isConvert;

        public void OnConvert()
        {
            foreach (var info in list)
            {
                if (string.IsNullOrEmpty(info.name))
                {
                    info.Clear();
                    continue;
                }

                info.hash = info.hashType switch
                {
                    HashType.Animator => Animator.StringToHash(info.name),
                    HashType.ShaderProperty => Shader.PropertyToID(info.name),
                    _ => 0,
                };
                 
            }
        }
    }
}
