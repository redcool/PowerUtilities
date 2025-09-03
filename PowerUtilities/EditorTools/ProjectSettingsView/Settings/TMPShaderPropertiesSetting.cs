using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    [ProjectSettingGroup(ProjectSettingGroupAttribute.POWER_UTILS + "/TMP Shader/TMPShaderPropertiesSetting")]
    [SOAssetPath(nameof(TMPShaderPropertiesSetting))]
    public class TMPShaderPropertiesSetting : ScriptableObject
    {
        [Header("Power's TMP shader properties ,show in * States")]
        public GUIContent[] stateContents = new[]
        {
            new GUIContent("_PresetBlendMode"),
            new GUIContent("_ZWriteMode"),
            new GUIContent("_ZTestMode"),
            new GUIContent("_CullMode"),
            new GUIContent("_GrayOn"),
            new GUIContent("_FaceColorRange"),
            new GUIContent("_FaceColor2"),

        };
}
}
